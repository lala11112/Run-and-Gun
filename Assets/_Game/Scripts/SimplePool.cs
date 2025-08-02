using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// [DEPRECATED] 간단한 GameObject 풀링 유틸리티.
/// AdvancedObjectPool로 대체되었습니다. 호환성을 위해 유지됩니다.
/// • 프리팹별 Queue를 관리하여 Instantiate/Destroy 비용을 줄입니다.
/// • 싱글 플레이, 1인 개발 규모에 맞춰 가볍게 설계되었습니다.
/// </summary>
[System.Obsolete("SimplePool은 더 이상 사용되지 않습니다. AdvancedObjectPool을 사용하세요.")]
public static class SimplePool
{
    // 프리팹 InstanceID → 풀 큐
    private static readonly Dictionary<int, Queue<GameObject>> _pools = new();

    /// <summary>
    /// 프리팹으로부터 오브젝트를 가져옵니다. (AdvancedObjectPool로 리다이렉트)
    /// </summary>
    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        // AdvancedObjectPool로 리다이렉트
        return AdvancedObjectPool.Spawn(prefab, position, rotation);
    }

    /// <summary>
    /// 사용이 끝난 오브젝트를 풀로 돌려보냅니다. (AdvancedObjectPool로 리다이렉트)
    /// </summary>
    public static void Despawn(GameObject obj)
    {
        // AdvancedObjectPool로 리다이렉트
        AdvancedObjectPool.Despawn(obj);
    }

    /// <summary>
    /// 풀에 남아있는 오브젝트 수를 반환합니다 (AdvancedObjectPool로 리다이렉트).
    /// </summary>
    public static int GetPoolCount(GameObject prefab)
    {
        var stats = AdvancedObjectPool.GetPoolStats(prefab);
        return stats.available;
    }

    /// <summary>
    /// 풀 초기화 (AdvancedObjectPool로 리다이렉트)
    /// </summary>
    public static void ClearAll()
    {
        AdvancedObjectPool.ClearAllPools();
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