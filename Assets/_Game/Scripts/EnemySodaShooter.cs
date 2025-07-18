using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 플레이어를 추적하면서 일정 간격으로 '탄산총' 버스트를 발사하는 적.
/// - Enemy.cs 와 함께 동일 오브젝트에 부착.
/// - 스턴 시 사격 중지.
/// </summary>
[RequireComponent(typeof(Enemy))]
public class EnemySodaShooter : MonoBehaviour
{
    [Header("감지 및 이동")]
    [Tooltip("플레이어 인식 거리")] public float detectionRange = 10f;
    [Tooltip("버스트당 발사할 총알 개수")] public int burstCount = 5;
    [Tooltip("버스트 간격(초)")] public float burstInterval = 1.2f;
    [Tooltip("버스트 내부 총알 간 딜레이(초)")] public float intraDelay = 0.08f;
    [Tooltip("총알 속도")] public float projectileSpeed = 7f;
    [Tooltip("버스트 퍼짐 각도(°)")] public float spreadAngle = 20f;
    [Tooltip("발사 위치")] public Transform firePoint;

    [Header("소다건 설정")]
    [Tooltip("발사할 투사체 프리팹 (EnemyProjectile)")] public GameObject bubbleProjectilePrefab;

    private Enemy _enemy;
    private Transform _player;
    private NavMeshAgent _agent;
    private bool _isBursting;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _player = GameObject.FindWithTag("Player")?.transform;
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (_player == null) return;
        if (_enemy.IsStunned) { if (_agent != null) _agent.isStopped = true; return; }

        float dist = Vector2.Distance(transform.position, _player.position);
        if (dist > detectionRange) return;

        // 이동 및 사격 제어 (NavMeshAgent 사용)
        if (_agent != null)
        {
            float keepDist = _enemy != null ? _enemy.keepDistance : 5f;
            if (dist > keepDist)
            {
                _agent.isStopped = false;
                _agent.speed = _enemy != null ? _enemy.moveSpeed : 2f;
                _agent.SetDestination(_player.position);
            }
            else
            {
                _agent.isStopped = true;
                if (!_isBursting) StartCoroutine(BurstRoutine());
            }
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