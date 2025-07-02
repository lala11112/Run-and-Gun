using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 플레이어를 일정 사거리에서 추적하다가, 사거리 안에 들어오면 이동을 멈추고 플레이어를 향해 연속으로 투사체를 발사하는 적.
/// Enemy.cs 와 함께 동일 오브젝트에 부착하여 사용한다.
/// </summary>
[RequireComponent(typeof(Enemy))]
public class EnemyShooter : MonoBehaviour
{
    [Header("Shooter Settings")]
    [Tooltip("플레이어를 인식하는 거리")] public float detectionRange = 10f;
    [Tooltip("사격을 시작하는 거리 (이 거리 이내로 들어오면 멈추고 사격)")] public float shootRange = 7f;
    [Tooltip("발사 간격(초)")] public float fireInterval = 0.4f;
    [Tooltip("발사할 투사체 프리팹 (EnemyProjectile)")] public GameObject projectilePrefab;
    [Tooltip("투사체 발사 속도, 0 이면 prefab 기본값 사용")] public float projectileSpeedOverride = 0f;
    [Tooltip("투사체를 발사할 위치 (Enemy 기준 상대 위치)")] public Transform firePoint;

    private Transform _player;
    private Enemy _enemy;
    private NavMeshAgent _agent;
    private float _fireTimer;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _agent = GetComponent<NavMeshAgent>();
        _player = GameObject.FindWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (_player == null) return;

        if (_enemy != null && _enemy.IsStunned) { if (_agent != null) _agent.isStopped = true; return; }

        float dist = Vector2.Distance(transform.position, _player.position);

        // 플레이어를 인식하지 못할 정도로 멀리 있으면 아무 것도 안 함
        if (dist > detectionRange) return;

        // 사격 사거리 내에 있으면 NavMeshAgent 정지 후 사격
        if (dist <= shootRange)
        {
            if (_agent != null) _agent.isStopped = true;
            HandleShooting();
        }
        else
        {
            if (_agent != null)
            {
                _agent.isStopped = false;
                _agent.SetDestination(_player.position);
            }
        }
    }

    private void HandleShooting()
    {
        if (projectilePrefab == null) return;

        _fireTimer += Time.deltaTime;
        if (_fireTimer < fireInterval) return;
        _fireTimer = 0f;

        Vector2 dir = (_player.position - transform.position).normalized;
        Transform spawnPoint = firePoint != null ? firePoint : transform;
        GameObject obj = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
        if (obj.TryGetComponent(out EnemyProjectile ep))
        {
            ep.Init(dir);
            if (projectileSpeedOverride > 0f)
            {
                ep.speed = projectileSpeedOverride;
            }
        }
    }
} 