using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 고성능 Object Pool 시스템입니다.
/// 기존 SimplePool을 개선하여 더 나은 성능과 메모리 효율성을 제공합니다.
/// </summary>
public static class AdvancedObjectPool
{
    /// <summary>
    /// 풀 정보를 담는 클래스입니다.
    /// </summary>
    public class PoolInfo
    {
        public Queue<GameObject> availableObjects = new Queue<GameObject>();
        public HashSet<GameObject> allObjects = new HashSet<GameObject>(); // 모든 풀 오브젝트 추적
        public GameObject prefab;
        public Transform poolParent; // 풀 오브젝트들의 부모 Transform
        public int maxSize;
        
        public PoolInfo(GameObject prefab, int maxSize)
        {
            this.prefab = prefab;
            this.maxSize = maxSize;
            
            // 풀 전용 부모 오브젝트 생성 (씬 정리용)
            var poolParentObj = new GameObject($"Pool_{prefab.name}");
            poolParentObj.SetActive(false); // 부모는 비활성화하여 성능 향상
            this.poolParent = poolParentObj.transform;
        }
    }
    
    private static readonly Dictionary<int, PoolInfo> _pools = new Dictionary<int, PoolInfo>();
    private static GameConfigSO _gameConfig;
    
    /// <summary>
    /// 풀을 초기화합니다. (선택적 - 미리 오브젝트들을 생성)
    /// </summary>
    public static void PrewarmPool(GameObject prefab, int count)
    {
        if (prefab == null) return;
        
        int prefabId = prefab.GetInstanceID();
        var poolInfo = GetOrCreatePool(prefabId, prefab);
        
        // 미리 지정된 개수만큼 오브젝트 생성
        for (int i = 0; i < count && poolInfo.allObjects.Count < poolInfo.maxSize; i++)
        {
            var obj = Object.Instantiate(prefab, poolInfo.poolParent);
            obj.SetActive(false);
            
            // 풀 마커 추가
            var marker = obj.GetComponent<AdvancedPoolMarker>();
            if (marker == null)
            {
                marker = obj.AddComponent<AdvancedPoolMarker>();
            }
            marker.prefabId = prefabId;
            marker.poolInfo = poolInfo;
            
            poolInfo.availableObjects.Enqueue(obj);
            poolInfo.allObjects.Add(obj);
        }
    }
    
    /// <summary>
    /// 프리팹으로부터 오브젝트를 가져옵니다.
    /// </summary>
    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (prefab == null)
        {
            Debug.LogError("[AdvancedObjectPool] Null 프리팹 요청");
            return null;
        }
        
        int prefabId = prefab.GetInstanceID();
        var poolInfo = GetOrCreatePool(prefabId, prefab);
        
        GameObject obj;
        
        // 사용 가능한 오브젝트가 있으면 재사용
        if (poolInfo.availableObjects.Count > 0)
        {
            obj = poolInfo.availableObjects.Dequeue();
            
            // Null 체크 (오브젝트가 외부에서 파괴된 경우)
            if (obj == null)
            {
                // 파괴된 오브젝트를 추적 목록에서 제거
                poolInfo.allObjects.Remove(obj);
                return Spawn(prefab, position, rotation, parent); // 재귀 호출
            }
        }
        else
        {
            // 풀이 최대 크기에 도달했으면 새로 생성하지 않고 가장 오래된 것 재사용
            if (poolInfo.allObjects.Count >= poolInfo.maxSize)
            {
                Debug.LogWarning($"[AdvancedObjectPool] {prefab.name} 풀이 최대 크기({poolInfo.maxSize})에 도달했습니다. 새 오브젝트를 생성하지 않습니다.");
                return null;
            }
            
            // 새 오브젝트 생성
            obj = Object.Instantiate(prefab, poolInfo.poolParent);
            
            // 풀 마커 추가
            var marker = obj.GetComponent<AdvancedPoolMarker>();
            if (marker == null)
            {
                marker = obj.AddComponent<AdvancedPoolMarker>();
            }
            marker.prefabId = prefabId;
            marker.poolInfo = poolInfo;
            
            poolInfo.allObjects.Add(obj);
        }
        
        // 오브젝트 활성화 및 위치 설정
        obj.transform.SetParent(parent);
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        
        return obj;
    }
    
    /// <summary>
    /// 오브젝트를 풀로 반환합니다.
    /// </summary>
    public static void Despawn(GameObject obj)
    {
        if (obj == null) return;
        
        var marker = obj.GetComponent<AdvancedPoolMarker>();
        if (marker == null || marker.poolInfo == null)
        {
            // 호환성 체크: SimplePool 마커가 있는지 확인
            var simpleMarker = obj.GetComponent<SimplePoolMarker>();
            if (simpleMarker != null)
            {
                // SimplePool에서 생성된 오브젝트는 그냥 파괴
                Debug.Log($"[AdvancedObjectPool] SimplePool 오브젝트 '{obj.name}' 파괴 (호환성 모드)");
                Object.Destroy(obj);
                return;
            }
            
            // 마커가 전혀 없는 오브젝트 - 일반 파괴
            Debug.LogWarning($"[AdvancedObjectPool] 풀링되지 않은 오브젝트 '{obj.name}'를 Despawn 시도 - 파괴합니다");
            Object.Destroy(obj);
            return;
        }
        
        var poolInfo = marker.poolInfo;
        
        // 이미 풀에 반환된 오브젝트인지 확인
        if (!obj.activeInHierarchy)
        {
            return; // 이미 비활성화된 오브젝트는 처리하지 않음
        }
        
        // 오브젝트를 풀 부모로 이동하고 비활성화
        obj.transform.SetParent(poolInfo.poolParent);
        obj.SetActive(false);
        
        // 풀에 반환
        poolInfo.availableObjects.Enqueue(obj);
    }
    
    /// <summary>
    /// 지연된 Despawn (코루틴 없이 구현)
    /// </summary>
    public static void DespawnAfter(GameObject obj, float delay)
    {
        if (obj == null) return;
        
        var marker = obj.GetComponent<AdvancedPoolMarker>();
        if (marker != null)
        {
            marker.StartCoroutine(marker.DespawnAfterDelay(delay));
        }
    }
    
    private static PoolInfo GetOrCreatePool(int prefabId, GameObject prefab)
    {
        if (!_pools.TryGetValue(prefabId, out var poolInfo))
        {
            // GameConfig에서 기본 풀 크기 가져오기
            if (_gameConfig == null)
            {
                _gameConfig = Resources.Load<GameConfigSO>("GameConfig");
            }
            
            int maxSize = _gameConfig != null ? _gameConfig.defaultPoolSize : 20;
            poolInfo = new PoolInfo(prefab, maxSize);
            _pools[prefabId] = poolInfo;
        }
        return poolInfo;
    }
    
    /// <summary>
    /// 풀 통계 정보를 반환합니다.
    /// </summary>
    public static (int available, int total, int max) GetPoolStats(GameObject prefab)
    {
        if (prefab == null) return (0, 0, 0);
        
        int prefabId = prefab.GetInstanceID();
        if (_pools.TryGetValue(prefabId, out var poolInfo))
        {
            return (poolInfo.availableObjects.Count, poolInfo.allObjects.Count, poolInfo.maxSize);
        }
        return (0, 0, 0);
    }
    
    /// <summary>
    /// 모든 풀을 정리합니다. (씬 전환 시 호출)
    /// </summary>
    public static void ClearAllPools()
    {
        foreach (var poolInfo in _pools.Values)
        {
            if (poolInfo.poolParent != null)
            {
                Object.Destroy(poolInfo.poolParent.gameObject);
            }
        }
        _pools.Clear();
    }
    
    /// <summary>
    /// 씬에 남아있는 오래된 풀링 오브젝트들을 정리합니다.
    /// </summary>
    public static void CleanupLegacyObjects()
    {
        // SimplePool 마커가 있는 오브젝트들 정리
        var simpleMarkers = Object.FindObjectsOfType<SimplePoolMarker>();
        foreach (var marker in simpleMarkers)
        {
            if (marker != null && marker.gameObject != null)
            {
                Debug.Log($"[AdvancedObjectPool] 레거시 SimplePool 오브젝트 '{marker.gameObject.name}' 정리");
                Object.Destroy(marker.gameObject);
            }
        }
        
        // 마커 없이 Clone이 붙은 오브젝트들 중 풀링 대상으로 보이는 것들 정리
        var allObjects = Object.FindObjectsOfType<GameObject>();
        foreach (var obj in allObjects)
        {
            if (obj.name.Contains("(Clone)") && 
                (obj.name.Contains("Projectile") || obj.name.Contains("Bullet") || obj.name.Contains("Effect")))
            {
                var advancedMarker = obj.GetComponent<AdvancedPoolMarker>();
                var simpleMarker = obj.GetComponent<SimplePoolMarker>();
                
                // 마커가 없는 풀링 대상 오브젝트는 정리
                if (advancedMarker == null && simpleMarker == null)
                {
                    Debug.Log($"[AdvancedObjectPool] 마커 없는 풀링 대상 오브젝트 '{obj.name}' 정리");
                    Object.Destroy(obj);
                }
            }
        }
    }
    
    /// <summary>
    /// 특정 풀만 정리합니다.
    /// </summary>
    public static void ClearPool(GameObject prefab)
    {
        if (prefab == null) return;
        
        int prefabId = prefab.GetInstanceID();
        if (_pools.TryGetValue(prefabId, out var poolInfo))
        {
            if (poolInfo.poolParent != null)
            {
                Object.Destroy(poolInfo.poolParent.gameObject);
            }
            _pools.Remove(prefabId);
        }
    }
}

/// <summary>
/// AdvancedObjectPool에서 사용하는 마커 컴포넌트입니다.
/// </summary>
public class AdvancedPoolMarker : MonoBehaviour
{
    [System.NonSerialized]
    public int prefabId;
    
    [System.NonSerialized]
    public AdvancedObjectPool.PoolInfo poolInfo;
    
    /// <summary>
    /// 지연된 Despawn을 위한 코루틴입니다.
    /// </summary>
    public System.Collections.IEnumerator DespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        AdvancedObjectPool.Despawn(gameObject);
    }
    
    private void OnDestroy()
    {
        // 오브젝트가 파괴될 때 풀에서 제거
        if (poolInfo != null)
        {
            poolInfo.allObjects.Remove(gameObject);
        }
    }
}