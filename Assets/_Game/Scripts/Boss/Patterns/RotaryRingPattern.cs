using System.Collections;
using UnityEngine;

/// <summary>
/// [Rotary Ring] – 원형 회전 탄막.
/// </summary>
public class RotaryRingPattern : MonoBehaviour, IBossPattern
{
    [Tooltip("투사체 프리팹")] public GameObject bulletPrefab;
    [Tooltip("회전 방향: true=시계, false=반시계")] public bool clockwise = true;
    [Tooltip("총 탄막 라운드 수")] public int rounds = 1;
    [Tooltip("한 라운드당 발사하는 탄 개수")] public int bulletsPerRound = 20;
    [Tooltip("회전 속도(도/초)")] public float rotationSpeed = 90f;
    [Tooltip("발사 간격")] public float fireInterval = 0.1f;
    [Tooltip("탄속")] public float bulletSpeed = 6f;

    public IEnumerator ExecutePattern()
    {
        float curAngle = 0f;
        int totalShots = rounds * bulletsPerRound;
        for (int i = 0; i < totalShots; i++)
        {
            Vector2 dir = Quaternion.Euler(0, 0, curAngle) * Vector2.down;
            GameObject obj = SimplePool.Spawn(bulletPrefab, transform.position, Quaternion.identity);
            if (obj.TryGetComponent(out EnemyProjectile ep))
            {
                ep.Init(dir);
                ep.speed = bulletSpeed;
            }
            curAngle += (clockwise ? 1 : -1) * 360f / bulletsPerRound;
            if (Mathf.Abs(curAngle) >= 360f) curAngle = 0f;
            yield return new WaitForSeconds(fireInterval);
        }
        yield return new WaitForSeconds(0.4f);
    }
} 