using UnityEngine;

/// <summary>
/// AdvancedObjectPool을 초기화하고 관리하는 매니저입니다.
/// 게임 시작 시 자주 사용되는 오브젝트들을 미리 풀에 생성합니다.
/// </summary>
public class AdvancedPoolManager : MonoBehaviour
{
    public static AdvancedPoolManager Instance { get; private set; }
    
    [Header("사전 로딩할 프리팹들")]
    [Tooltip("투사체 프리팹들입니다.")]
    public GameObject[] projectilePrefabs;
    
    [Tooltip("이펙트 프리팹들입니다.")]
    public GameObject[] effectPrefabs;
    
    [Header("설정")]
    [Tooltip("게임 설정 ScriptableObject입니다.")]
    public GameConfigSO gameConfig;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 레거시 오브젝트 정리 후 풀 초기화
        AdvancedObjectPool.CleanupLegacyObjects();
        InitializePools();
    }
    
    /// <summary>
    /// 풀들을 초기화하고 사전 로딩합니다.
    /// </summary>
    private void InitializePools()
    {
        if (gameConfig == null)
        {
            gameConfig = Resources.Load<GameConfigSO>("GameConfig");
            if (gameConfig == null)
            {
                Debug.LogWarning("[AdvancedPoolManager] GameConfig를 찾을 수 없습니다. 기본값을 사용합니다.");
                return;
            }
        }
        
        Debug.Log("[AdvancedPoolManager] Object Pool 초기화 시작...");
        
        // 투사체 풀 사전 로딩
        if (projectilePrefabs != null)
        {
            foreach (var prefab in projectilePrefabs)
            {
                if (prefab != null)
                {
                    AdvancedObjectPool.PrewarmPool(prefab, gameConfig.prewarmCount);
                    Debug.Log($"[AdvancedPoolManager] {prefab.name} 투사체 풀 사전 로딩 완료 ({gameConfig.prewarmCount}개)");
                }
            }
        }
        
        // 이펙트 풀 사전 로딩
        if (effectPrefabs != null)
        {
            foreach (var prefab in effectPrefabs)
            {
                if (prefab != null)
                {
                    AdvancedObjectPool.PrewarmPool(prefab, gameConfig.prewarmCount);
                    Debug.Log($"[AdvancedPoolManager] {prefab.name} 이펙트 풀 사전 로딩 완료 ({gameConfig.prewarmCount}개)");
                }
            }
        }
        
        Debug.Log("[AdvancedPoolManager] Object Pool 초기화 완료!");
    }
    
    /// <summary>
    /// 비동기 초기화 (로딩 화면에서 사용)
    /// </summary>
    public System.Collections.IEnumerator InitializePoolsAsync(System.Action<float> onProgress = null)
    {
        if (gameConfig == null)
        {
            gameConfig = Resources.Load<GameConfigSO>("GameConfig");
            if (gameConfig == null)
            {
                Debug.LogWarning("[AdvancedPoolManager] GameConfig를 찾을 수 없습니다. 기본값을 사용합니다.");
                yield break;
            }
        }
        
        Debug.Log("[AdvancedPoolManager] 비동기 Object Pool 초기화 시작...");
        
        int totalPrefabs = (projectilePrefabs?.Length ?? 0) + (effectPrefabs?.Length ?? 0);
        int processedPrefabs = 0;
        
        // 투사체 풀 사전 로딩
        if (projectilePrefabs != null)
        {
            foreach (var prefab in projectilePrefabs)
            {
                if (prefab != null)
                {
                    AdvancedObjectPool.PrewarmPool(prefab, gameConfig.prewarmCount);
                    Debug.Log($"[AdvancedPoolManager] {prefab.name} 투사체 풀 사전 로딩 완료");
                }
                processedPrefabs++;
                onProgress?.Invoke((float)processedPrefabs / totalPrefabs);
                yield return null; // 한 프레임 대기
            }
        }
        
        // 이펙트 풀 사전 로딩
        if (effectPrefabs != null)
        {
            foreach (var prefab in effectPrefabs)
            {
                if (prefab != null)
                {
                    AdvancedObjectPool.PrewarmPool(prefab, gameConfig.prewarmCount);
                    Debug.Log($"[AdvancedPoolManager] {prefab.name} 이펙트 풀 사전 로딩 완료");
                }
                processedPrefabs++;
                onProgress?.Invoke((float)processedPrefabs / totalPrefabs);
                yield return null; // 한 프레임 대기
            }
        }
        
        Debug.Log("[AdvancedPoolManager] 비동기 Object Pool 초기화 완료!");
    }
    
    /// <summary>
    /// 런타임에 새로운 프리팹을 풀에 추가합니다.
    /// </summary>
    public void AddToPool(GameObject prefab, int prewarmCount = 0)
    {
        if (prefab == null) return;
        
        if (prewarmCount > 0)
        {
            AdvancedObjectPool.PrewarmPool(prefab, prewarmCount);
            Debug.Log($"[AdvancedPoolManager] {prefab.name} 풀 추가 및 사전 로딩 완료 ({prewarmCount}개)");
        }
    }
    
    /// <summary>
    /// 풀 통계를 로그로 출력합니다. (디버그용)
    /// </summary>
    [ContextMenu("풀 통계 출력")]
    public void LogPoolStats()
    {
        Debug.Log("=== Object Pool 통계 ===");
        
        if (projectilePrefabs != null)
        {
            foreach (var prefab in projectilePrefabs)
            {
                if (prefab != null)
                {
                    var stats = AdvancedObjectPool.GetPoolStats(prefab);
                    Debug.Log($"{prefab.name}: 사용가능={stats.available}, 전체={stats.total}, 최대={stats.max}");
                }
            }
        }
        
        if (effectPrefabs != null)
        {
            foreach (var prefab in effectPrefabs)
            {
                if (prefab != null)
                {
                    var stats = AdvancedObjectPool.GetPoolStats(prefab);
                    Debug.Log($"{prefab.name}: 사용가능={stats.available}, 전체={stats.total}, 최대={stats.max}");
                }
            }
        }
    }
    
    /// <summary>
    /// 씬 전환 시 호출하여 풀을 정리합니다.
    /// </summary>
    public void ClearAllPools()
    {
        AdvancedObjectPool.CleanupLegacyObjects(); // 레거시 오브젝트 정리
        AdvancedObjectPool.ClearAllPools();
        Debug.Log("[AdvancedPoolManager] 모든 풀과 레거시 오브젝트가 정리되었습니다.");
    }
    
    /// <summary>
    /// 수동으로 레거시 오브젝트를 정리합니다. (디버그용)
    /// </summary>
    [ContextMenu("레거시 오브젝트 정리")]
    public void CleanupLegacyObjects()
    {
        AdvancedObjectPool.CleanupLegacyObjects();
        Debug.Log("[AdvancedPoolManager] 레거시 오브젝트 정리가 완료되었습니다.");
    }
    
    private void OnDestroy()
    {
        // 매니저가 파괴될 때 풀 정리
        AdvancedObjectPool.ClearAllPools();
    }
}