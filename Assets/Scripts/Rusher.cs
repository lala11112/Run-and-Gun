using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 돌진형 적 "러셔".
/// 탐지 → 경고 → 돌진 → 폭발 사이클을 반복합니다.
/// 돌진 중 X 스킬(넉백) / C 스킬(돌진) 등에 맞으면 스턴 후 즉시 자폭합니다.
/// </summary>
[RequireComponent(typeof(Enemy))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Rusher : MonoBehaviour
{
    private enum RusherState { Idle, Alert, Preparing, Charging, Cooldown }

    [Header("러셔 탐지 설정")]
    [Tooltip("플레이어를 감지할 거리")] public float alertRange = 5f;

    [Header("차지 준비 설정")]
    [Tooltip("차지 준비 최소 대기 시간")] public float minPrepareTime = 0.5f;
    [Tooltip("차지 준비 최대 대기 시간")] public float maxPrepareTime = 1.0f;
    [Tooltip("경고 이펙트 프리팹 (머리 위 '!!' 등)")] public GameObject alertEffectPrefab;

    [Header("차지 설정")]
    [Tooltip("돌진 시 속도 배수")] public float chargeSpeedMultiplier = 3f;
    [Tooltip("돌진 최대 지속 시간")] public float maxChargeTime = 1.5f;
    [Tooltip("충돌 태그: 벽, 장애물")] public string[] obstacleTags = { "Wall", "Obstacle" };

    [Header("데미지 설정")]
    [Tooltip("돌진 중 직접 충돌 데미지")] public int contactDamage = 1;
    [Tooltip("폭발 데미지")] public int explodeDamage = 1;

    [Header("프리-폭발 탄막 설정")]
    [Tooltip("폭발 직전 발사할 총알 프리팹")] public GameObject preExplodeBulletPrefab;
    [Tooltip("탄막 발사 속도")] public float preBulletSpeed = 6f;
    [Tooltip("폭발 직전 시간")]
    public float preExplodeTime = 0.2f;

    [Header("폭발 이펙트")]
    [Tooltip("자폭 이펙트 프리팹 (선택)")] public GameObject explodeEffectPrefab;

    private Enemy _enemy;
    private Rigidbody2D _rb;
    private NavMeshAgent _agent;
    private Transform _player;

    private RusherState _state = RusherState.Idle;
    private Coroutine _routine;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _rb = GetComponent<Rigidbody2D>();
        _agent = GetComponent<NavMeshAgent>();
        _player = GameObject.FindWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (_state == RusherState.Idle && _player != null)
        {
            float dist = Vector2.Distance(transform.position, _player.position);
            if (dist <= alertRange)
            {
                SwitchState(RusherState.Alert);
            }
        }
    }

    private void SwitchState(RusherState next)
    {
        if (_routine != null) StopCoroutine(_routine);
        _state = next;
        switch (next)
        {
            case RusherState.Alert:
                _routine = StartCoroutine(AlertRoutine());
                break;
            case RusherState.Preparing:
                _routine = StartCoroutine(PrepareRoutine());
                break;
            case RusherState.Charging:
                _routine = StartCoroutine(ChargeRoutine());
                break;
            case RusherState.Cooldown:
                _routine = StartCoroutine(CooldownRoutine());
                break;
        }
    }

    private IEnumerator AlertRoutine()
    {
        // 경고 이펙트 스폰
        if (alertEffectPrefab != null)
        {
            GameObject fx = Instantiate(alertEffectPrefab, transform.position + Vector3.up * 1.2f, Quaternion.identity, transform);
            Destroy(fx, 1.5f);
        }
        yield return new WaitForSeconds(0.2f);
        SwitchState(RusherState.Preparing);
    }

    private IEnumerator PrepareRoutine()
    {
        float wait = Random.Range(minPrepareTime, maxPrepareTime);
        // NavMeshAgent 정지 및 진동 애니메이션(간단히 scale 펄스)
        if (_agent != null) _agent.isStopped = true;
        float elapsed = 0f;
        Vector3 originalScale = transform.localScale;
        while (elapsed < wait)
        {
            elapsed += Time.deltaTime;
            float pulse = 1f + Mathf.Sin(elapsed * 40f) * 0.05f;
            transform.localScale = originalScale * pulse;
            yield return null;
        }
        transform.localScale = originalScale;
        SwitchState(RusherState.Charging);
    }

    private IEnumerator ChargeRoutine()
    {
        if (_agent != null) _agent.isStopped = true; // 경로 계산 중단
        Vector2 dir = (_player != null ? (_player.position - transform.position) : Vector3.up).normalized;
        _rb.linearVelocity = dir * (_enemy != null ? _enemy.moveSpeed * chargeSpeedMultiplier : 8f);
        float chargeTimer = 0f;
        bool bulletsFired = false;
        while (chargeTimer < maxChargeTime)
        {
            chargeTimer += Time.deltaTime;
            if (!bulletsFired && chargeTimer >= maxChargeTime - preExplodeTime)
            {
                bulletsFired = true;
                FirePreExplodeBullets(dir);
            }
            yield return null;
        }
        Explode();
    }

    private IEnumerator CooldownRoutine()
    {
        // 간단히 대기 후 Idle 복귀
        if (_agent != null) { _agent.isStopped = false; }
        _rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(1f);
        SwitchState(RusherState.Idle);
    }

    private void FirePreExplodeBullets(Vector2 forward)
    {
        if (preExplodeBulletPrefab == null) return;
        int count = 3;
        float spread = 15f;
        for (int i = 0; i < count; i++)
        {
            float angle = -spread + (spread * 2f / (count - 1)) * i;
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * forward;
            GameObject obj = Instantiate(preExplodeBulletPrefab, transform.position, Quaternion.identity);
            if (obj.TryGetComponent(out EnemyProjectile ep))
            {
                ep.Init(dir);
                ep.speed = preBulletSpeed;
            }
        }
    }

    private void Explode()
    {
        // 이펙트
        if (explodeEffectPrefab != null)
        {
            Instantiate(explodeEffectPrefab, transform.position, Quaternion.identity);
        }
        // 자폭 데미지 처리 – 플레이어가 가까이 있으면 피해
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, 1.2f);
        foreach (var c in cols)
        {
            if (c.TryGetComponent(out PlayerHealth ph)) ph.TakeDamage(explodeDamage);
        }
        // 자신 파괴
        if (_enemy != null)
        {
            _enemy.TakeDamage(int.MaxValue);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 돌진 중 충돌 처리
        if (_state == RusherState.Charging)
        {
            // 환경 오브젝트 또는 플레이어 충돌 시 폭발
            if (other.CompareTag("Player"))
            {
                if (other.TryGetComponent(out PlayerHealth ph)) ph.TakeDamage(contactDamage);
                Explode();
            }
            else if (System.Array.Exists(obstacleTags, t => other.CompareTag(t)))
            {
                Explode();
            }
        }
    }

    // 외부에서 Enemy.Stun 이 호출되면 자폭으로 이어지도록 처리
    private void OnEnable()
    {
        if (_enemy != null) _enemy.OnHealthChanged += OnEnemyHealthChanged;
    }
    private void OnDisable()
    {
        if (_enemy != null) _enemy.OnHealthChanged -= OnEnemyHealthChanged;
    }
    private void OnEnemyHealthChanged(int cur, int max) { /* no-op placeholder */ }

    public void OnStunnedBySkill()
    {
        // 스턴 중엔 즉시 폭발
        Explode();
    }
} 