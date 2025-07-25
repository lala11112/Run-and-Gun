using System.Collections;
using UnityEngine;

/// <summary>
/// [Target Lock] – 플레이어 추적 유도탄 4연발.
/// </summary>
public class TargetLockPattern : MonoBehaviour, IBossPattern
{
    [Tooltip("유도탄 프리팹")] public GameObject missilePrefab;
    [Tooltip("연발 횟수")] public int missileCount = 4;
    [Tooltip("발사 간격")] public float missileInterval = 0.6f;
    [Tooltip("미사일 속도")] public float missileSpeed = 5f;

    public IEnumerator ExecutePattern()
    {
        Transform player = GameObject.FindWithTag("Player")?.transform;
        for (int i = 0; i < missileCount; i++)
        {
            if (player == null) break;
            Vector2 dir = (player.position - transform.position).normalized;
            GameObject obj = SimplePool.Spawn(missilePrefab, transform.position, Quaternion.identity);
            if (obj.TryGetComponent(out EnemyProjectile ep))
            {
                ep.Init(dir);
                ep.speed = missileSpeed;
            }
            yield return new WaitForSeconds(missileInterval);
        }
        yield return new WaitForSeconds(0.5f);
    }
} 