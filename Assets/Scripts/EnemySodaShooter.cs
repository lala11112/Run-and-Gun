using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어를 추적하면서 일정 간격으로 '탄산총' 버스트를 발사하는 적.
/// - Enemy.cs 와 함께 동일 오브젝트에 부착.
/// - 스턴 시 사격 중지.
/// </summary>
[RequireComponent(typeof(Enemy))]
public class EnemySodaShooter : MonoBehaviour
{
    [Header("Detection & Movement")]
    [Tooltip("플레이어 인식 거리")] public float detectionRange = 10f;
    [Tooltip("사격 사거리")] public float shootRange = 8f;
    [Tooltip("이동 속도")] public float moveSpeed = 2f;

    [Header("Soda Gun Settings")]
    [Tooltip("발사할 투사체 프리팹 (EnemyProjectile)")] public GameObject bubbleProjectilePrefab;
    [Tooltip("버스트당 발사할 총알 개수")] public int burstCount = 5;
    [Tooltip("버스트 간격(초)")] public float burstInterval = 1.2f;
    [Tooltip("버스트 내부 총알 간 딜레이(초)")] public float intraDelay = 0.08f;
    [Tooltip("총알 속도")] public float projectileSpeed = 7f;
    [Tooltip("버스트 퍼짐 각도(°)")] public float spreadAngle = 20f;
    [Tooltip("발사 위치")] public Transform firePoint;

    private Enemy _enemy;
    private Transform _player;
    private Rigidbody2D _rb;
    private bool _isBursting;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _player = GameObject.FindWithTag("Player")?.transform;
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (_player == null) return;
        if (_enemy.IsStunned) { _rb.linearVelocity = Vector2.zero; return; }

        float dist = Vector2.Distance(transform.position, _player.position);
        if (dist > detectionRange) return;

        // 이동
        if (dist > shootRange)
        {
            Vector2 dir = (_player.position - transform.position).normalized;
            _rb.linearVelocity = dir * moveSpeed;
        }
        else
        {
            _rb.linearVelocity = Vector2.zero;
            if (!_isBursting) StartCoroutine(BurstRoutine());
        }
    }

    private IEnumerator BurstRoutine()
    {
        _isBursting = true;
        for (int i = 0; i < burstCount; i++)
        {
            FireBubble();
            yield return new WaitForSeconds(intraDelay);
        }
        yield return new WaitForSeconds(burstInterval - burstCount * intraDelay);
        _isBursting = false;
    }

    private void FireBubble()
    {
        if (bubbleProjectilePrefab == null) return;
        Vector2 toPlayer = (_player.position - transform.position).normalized;
        // 가벼운 퍼짐 적용
        float angleOffset = Random.Range(-spreadAngle * 0.5f, spreadAngle * 0.5f);
        Vector2 dir = Quaternion.Euler(0f, 0f, angleOffset) * toPlayer;
        Transform spawn = firePoint != null ? firePoint : transform;
        GameObject obj = Instantiate(bubbleProjectilePrefab, spawn.position, Quaternion.identity);
        if (obj.TryGetComponent(out EnemyProjectile ep))
        {
            ep.Init(dir);
            ep.speed = projectileSpeed;
        }
    }
} 