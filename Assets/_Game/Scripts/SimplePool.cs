using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 간단한 GameObject 풀링 유틸리티.
/// • 프리팹별 Queue를 관리하여 Instantiate/Destroy 비용을 줄입니다.
/// • 싱글 플레이, 1인 개발 규모에 맞춰 가볍게 설계되었습니다.
/// </summary>
public static class SimplePool
{
    // 프리팹 InstanceID → 풀 큐
    private static readonly Dictionary<int, Queue<GameObject>> _pools = new();

    /// <summary>
    /// 프리팹으로부터 오브젝트를 가져옵니다. (없으면 Instantiate)
    /// </summary>
    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            Debug.LogError("[SimplePool] Null 프리팹 요청");
            return null;
        }

        int id = prefab.GetInstanceID();
        GameObject obj;
        if (_pools.TryGetValue(id, out var q) && q.Count > 0)
        {
            obj = q.Dequeue();
            if (obj == null)
            {
                // Null 이면, 이미 Destroy 된 경우 – 재귀 호출 방지 위해 새로 생성
                return Spawn(prefab, position, rotation);
            }
        }
        else
        {
            obj = Object.Instantiate(prefab);
            obj.AddComponent<SimplePoolMarker>().prefabId = id; // 프리팹 ID 기록
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        return obj;
    }

    /// <summary>
    /// 사용이 끝난 오브젝트를 풀로 돌려보냅니다.
    /// </summary>
    public static void Despawn(GameObject obj)
    {
        if (obj == null) return;
        var idComp = obj.GetComponent<SimplePoolMarker>();
        if (idComp == null)
        {
            Debug.LogWarning("[SimplePool] 풀링되지 않은 오브젝트를 Despawn 시도");
            Object.Destroy(obj);
            return;
        }

        int id = idComp.prefabId;
        if (!_pools.TryGetValue(id, out var q))
        {
            q = new Queue<GameObject>();
            _pools.Add(id, q);
        }
        // 비활성화 후 큐에 보관
        obj.SetActive(false);
        q.Enqueue(obj);
    }

    /// <summary>
    /// 풀에 남아있는 오브젝트 수를 반환합니다 (디버그 용).
    /// </summary>
    public static int GetPoolCount(GameObject prefab)
    {
        if (prefab == null) return 0;
        if (_pools.TryGetValue(prefab.GetInstanceID(), out var q))
        {
            return q.Count;
        }
        return 0;
    }

    /// <summary>
    /// 풀 초기화 (씬 전환 시 호출 권장)
    /// </summary>
    public static void ClearAll()
    {
        foreach (var kv in _pools)
        {
            var q = kv.Value;
            while (q.Count > 0)
            {
                var obj = q.Dequeue();
                if (obj != null)
                {
                    Object.Destroy(obj);
                }
            }
        }
        _pools.Clear();
    }
}

/// <summary>
/// SimplePool에서 사용하는 식별자 컴포넌트.
/// 퍼블릭 클래스로 분리하여 Unity가 직렬화 가능하도록 합니다.
/// </summary>
public class SimplePoolMarker : MonoBehaviour
{
    public int prefabId;
} 