using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 간단한 GameObject 풀 매니저. prefab 단위로 Queue 를 보유합니다.
/// </summary>
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();

    [Tooltip("초기 풀 크기 (선택)")] public int defaultCapacity = 10;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// prefab 을 위치·회전으로 스폰. 풀에 남는 오브젝트가 없으면 새로 Instantiate.
    /// </summary>
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;
        if (!_pools.TryGetValue(prefab, out var queue) || queue.Count == 0)
        {
            var obj = Instantiate(prefab, position, rotation);
            return obj;
        }
        else
        {
            var obj = queue.Dequeue();
            var tr = obj.transform;
            tr.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            return obj;
        }
    }

    /// <summary>
    /// 사용이 끝난 인스턴스를 풀에 반환.
    /// </summary>
    public void Despawn(GameObject prefab, GameObject instance)
    {
        if (prefab == null || instance == null) return;
        if (!_pools.TryGetValue(prefab, out var queue))
        {
            queue = new Queue<GameObject>(defaultCapacity);
            _pools[prefab] = queue;
        }
        instance.SetActive(false);
        queue.Enqueue(instance);
    }
}