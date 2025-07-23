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
        for (int v = 0; v < volleyCount; v++)
        {
            for (int i = 0; i < bulletPerVolley; i++)
            {
                float t = bulletPerVolley == 1 ? 0 : (float)i / (bulletPerVolley - 1);
                float ang = -spreadAngle * 0.5f + t * spreadAngle;
                Vector2 dir = Quaternion.Euler(0, 0, ang) * Vector2.down; // 보스가 위에 있다고 가정

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