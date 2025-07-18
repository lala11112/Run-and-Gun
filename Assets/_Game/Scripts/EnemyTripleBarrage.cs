using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어가 인식 범위에 들어오면 제자리에서 플레이어를 중심으로 탄막을 세 번 발사하는 적.
/// Enemy.cs와 함께 사용하며, EnemyProjectile을 이용해 탄막을 구성.
/// </summary>
[RequireComponent(typeof(Enemy))]
public class EnemyTripleBarrage : MonoBehaviour
{
    [Header("탄막 설정")]
    [Tooltip("플레이어 인식 거리")] public float detectionRange = 12f;
    [Tooltip("원형 탄막 1회당 발사할 총알 개수")] public int bulletsPerRing = 16;
    [Tooltip("탄막(원) 간 딜레이(초)")] public float ringInterval = 0.35f;
    [Tooltip("발사할 총 원형 탄막 개수")] public int ringCount = 3;
    [Tooltip("발사할 투사체 프리팹")] public GameObject projectilePrefab;
    [Tooltip("투사체 속도 덮어쓰기(0이면 기본값)")] public float projectileSpeedOverride = 0f;
    [Tooltip("투사체가 발사될 기준 위치(없으면 자신 Transform)")] public Transform firePoint;

    private Transform _player;
    private bool _isFiring;

    private void Awake()
    {
        _player = GameObject.FindWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (_player == null || _isFiring) return;

        if (TryGetComponent(out Enemy enemyComp) && enemyComp.IsStunned) return;

        float dist = Vector2.Distance(transform.position, _player.position);
        if (dist <= detectionRange)
        {
            StartCoroutine(FireTripleBarrage());
        }
    }

    private IEnumerator FireTripleBarrage()
    {
        _isFiring = true;
        for (int ring = 0; ring < ringCount; ring++)
        {
            FireRing();
            yield return new WaitForSeconds(ringInterval);
        }
        _isFiring = false;
    }

    private void FireRing()
    {
        if (projectilePrefab == null) return;

        Vector2 center = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        float angleStep = 360f / bulletsPerRing;
        float currentAngle = 0f;

        for (int i = 0; i < bulletsPerRing; i++)
        {
            float rad = currentAngle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
            Vector2 spawnPos = center + dir * 0.1f; // 플레이어 바로 근처에서 생성

            GameObject obj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            if (obj.TryGetComponent(out EnemyProjectile ep))
            {
                ep.Init(dir);
                if (projectileSpeedOverride > 0f)
                {
                    ep.speed = projectileSpeedOverride;
                }
            }
            currentAngle += angleStep;
        }
    }
} 