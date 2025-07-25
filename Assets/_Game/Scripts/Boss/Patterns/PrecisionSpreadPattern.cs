using System.Collections;
using UnityEngine;

/// <summary>
/// [Precision Spread] – 부채꼴 탄막 2연발 (Phase1 기본 패턴).
/// </summary>
public class PrecisionSpreadPattern : MonoBehaviour, IBossPattern
{
    [Header("탄막 설정")] [Tooltip("발사할 투사체 프리팹")] public GameObject bulletPrefab;
    [Tooltip("1연발당 탄 개수")] public int bulletPerVolley = 12;
    [Tooltip("부채꼴 각도")] public float spreadAngle = 60f;
    [Tooltip("연발 횟수")] public int volleyCount = 2;
    [Tooltip("발사 간격(초)")] public float volleyInterval = 0.5f;
    [Tooltip("탄속")] public float bulletSpeed = 8f;

    public IEnumerator ExecutePattern()
    {
        Transform origin = transform;
        Transform player = GameObject.FindWithTag("Player")?.transform;
        Vector2 baseDir = player != null ? (player.position - origin.position).normalized : Vector2.down;
        for (int v = 0; v < volleyCount; v++)
        {
            // 발사 전 매 volley마다 최신 플레이어 방향 갱신 (이동 대응)
            if (player != null) baseDir = ((Vector2)(player.position - origin.position)).normalized;

            for (int i = 0; i < bulletPerVolley; i++)
            {
                float t = bulletPerVolley == 1 ? 0 : (float)i / (bulletPerVolley - 1);
                float angOffset = -spreadAngle * 0.5f + t * spreadAngle;
                Vector2 dir = Quaternion.AngleAxis(angOffset, Vector3.forward) * baseDir;

                GameObject obj = SimplePool.Spawn(bulletPrefab, origin.position, Quaternion.identity);
                if (obj.TryGetComponent(out EnemyProjectile ep))
                {
                    ep.Init(dir);
                    ep.speed = bulletSpeed;
                }
            }
            yield return new WaitForSeconds(volleyInterval);
        }
        // 패턴 종료 후 다음으로 넘어가도록 0.3초 대기
        yield return new WaitForSeconds(0.3f);
    }
} 