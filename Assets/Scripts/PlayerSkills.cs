using System.Collections;
using UnityEngine;

/// <summary>
/// SkillManager에서 발생하는 스킬 사용 이벤트를 받아 실제 효과를 수행하는 컴포넌트
/// 플레이어 오브젝트에 부착
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerSkills : MonoBehaviour
{
    private PlayerController _pc;
    private bool _isSubscribed;

    [Header("W Skill Settings")]
    [Tooltip("W 스킬 회전 공격의 범위 반경 (단위)")]
    public float wRadius = 3f;
    [Tooltip("W 스킬 정상 데미지")]
    public int wDamage = 2;
    [Tooltip("W 스킬 연속 사용 패널티 시 데미지")]
    public int wDamageWeakened = 1;
    [Tooltip("궁극기 상태에서 회전 공격 반복 횟수")]
    public int wUltimateRepeats = 3;
    [Tooltip("W 스킬 범위를 표시할 이펙트 프리팹 (파티클, 원형 스프라이트 등)")]
    public GameObject wEffectPrefab;
    [Tooltip("이펙트 유지 시간 (초) - 반복당")]
    public float wEffectDuration = 0.3f;

    [Header("W Skill Penalty Settings")]
    [Tooltip("같은 W 스킬을 연속 사용 시, 한 번 사용할 때마다 범위가 줄어드는 비율 (0~1)")]
    public float wRadiusPenaltyStep = 0.2f; // 0.2 = 20% 감소
    [Tooltip("패널티로 줄어드는 최소 반경 한계값")]
    public float wRadiusMin = 0.5f;

    // 내부 카운터: 연속 W 사용 횟수 (첫 사용 = 1)
    private int _wConsecutiveCount = 0;

    [Header("Q Skill Settings")]
    [Tooltip("Q 스킬 투사체 프리팹 (QProjectile)")]
    public GameObject qProjectilePrefab;
    [Tooltip("Q 스킬 투사체 발사 횟수")]
    public int qProjectileCount = 8;
    [Tooltip("Q 스킬 투사체 발사 간격 (초)")]
    public float qFireInterval = 0.07f;
    [Tooltip("Q 스킬 이동 속도 배수")]
    public float qSpeedMultiplier = 1.8f;
    [Tooltip("마우스에 적이 없을 때 탐색할 최대 반경")] public float qTargetSearchRadius = 20f;
    [Tooltip("이동 속도 증가 유지 시간 (초)")] public float qSpeedDuration = 3f;

    [Header("E Dash Penalty Settings")]
    [Tooltip("같은 E 스킬(대시)을 연속 사용 시 거리(속도) 감소 비율 (0~1)")]
    public float eDashPenaltyStep = 0.2f;
    [Tooltip("대시 감소 최소 배수")]
    public float eDashMinMultiplier = 0.4f;

    private int _eConsecutiveCount = 0;

    private int _qSpeedBuffCount = 0;
    private float _baseMoveSpeed;

    [Header("R Skill Settings")]
    [Tooltip("R 스킬 갈고리 프리팹(HookProjectile)")] public GameObject rHookPrefab;
    [Tooltip("갈고리 속도")] public float rHookSpeed = 20f;
    [Tooltip("갈고리 최대 거리")] public float rHookMaxDistance = 12f;
    [Tooltip("몸통 박치기 데미지")] public int rCollisionDamage = 3;
    [Tooltip("플레이어가 튕겨져 나갈 때 적용할 임펄스 힘")] public float rBounceForce = 12f;
    [Tooltip("벽에 갈고리가 박혔을 때 재사용 시 벽으로 이동 속도 배수")] public float rWallPullMultiplier = 1.4f;
    [Tooltip("적 돌진 시 거리 계산 배수(속도 조정용)")] public float rEnemyDashExtraMultiplier = 1.1f;

    private HookProjectile _activeHook;
    private bool _hookLatchedWall;
    private Vector2 _wallPoint;
    private Vector2 _lastHookDir;

    private bool _hookLatchedEnemy;
    private Enemy _enemyTarget;

    private void Awake()
    {
        _pc = GetComponent<PlayerController>();
        _baseMoveSpeed = _pc.moveSpeed;
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        // Start에서도 한 번 더 시도 (Awake 호출 순서로 인해 OnEnable에서 실패할 수 있음)
        TrySubscribe();
    }

    private void TrySubscribe()
    {
        if (_isSubscribed) return;
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.OnSkillActivated += OnSkillActivated;
            _isSubscribed = true;
        }
    }

    private void OnDisable()
    {
        if (_isSubscribed && SkillManager.Instance != null)
        {
            SkillManager.Instance.OnSkillActivated -= OnSkillActivated;
            _isSubscribed = false;
        }
    }

    private void OnSkillActivated(SkillType type, bool weakened)
    {
        // 연속 W 사용 카운트 관리
        if (type == SkillType.W)
        {
            _wConsecutiveCount++;
        }
        else
        {
            _wConsecutiveCount = 0;
        }

        if (type == SkillType.E)
        {
            _eConsecutiveCount++;
        }
        else
        {
            _eConsecutiveCount = 0;
        }

        switch (type)
        {
            case SkillType.Q:
                StartCoroutine(Q_StunSkill(weakened));
                break;
            case SkillType.W:
                StartCoroutine(W_SpinAttack(weakened));
                break;
            case SkillType.E:
                E_Dash();
                break;
            case SkillType.R:
                R_Barrage();
                break;
        }
    }

    #region Q Skill – Stun + Speed Buff
    private IEnumerator Q_StunSkill(bool weakened)
    {
        float stunDuration = weakened ? 1f : 2f;
        if (SkillManager.Instance.IsUltimateActive) stunDuration += 1f;

        // 마우스 위치의 적 하나 지정
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = mouseWorld;

        Collider2D[] hits = Physics2D.OverlapPointAll(mousePos2D);
        Enemy target = null;
        foreach (var h in hits)
        {
            if (h.TryGetComponent(out Enemy enemy))
            {
                target = enemy;
                break;
            }
        }

        // 만약 마우스에 적이 없다면, 기존 방식으로 주변 가장 가까운 적 찾기
        if (target == null)
        {
            Collider2D[] nearHits = Physics2D.OverlapCircleAll(transform.position, qTargetSearchRadius);
            float minDist = float.MaxValue;
            foreach (var h in nearHits)
            {
                if (h.TryGetComponent(out Enemy enemy))
                {
                    float d = Vector2.Distance(transform.position, enemy.transform.position);
                    if (d < minDist)
                    {
                        minDist = d;
                        target = enemy;
                    }
                }
            }
        }

        if (target != null)
        {
            target.Stun(stunDuration);

            // 투사체 연사 코루틴 실행
            if (qProjectilePrefab != null && _pc.firePoint != null)
            {
                StartCoroutine(Q_FireBarrage(target));
            }
        }

        _qSpeedBuffCount++;
        if (_qSpeedBuffCount == 1)
        {
            _pc.moveSpeed = _baseMoveSpeed * qSpeedMultiplier;
        }

        yield return new WaitForSeconds(qSpeedDuration);

        _qSpeedBuffCount--;
        if (_qSpeedBuffCount <= 0)
        {
            _qSpeedBuffCount = 0;
            _pc.moveSpeed = _baseMoveSpeed;
        }
    }

    private IEnumerator Q_FireBarrage(Enemy target)
    {
        for (int i = 0; i < qProjectileCount; i++)
        {
            if (target == null) break;
            Vector2 dir = (target.transform.position - _pc.firePoint.position).normalized;
            GameObject obj = Instantiate(qProjectilePrefab, _pc.firePoint.position, Quaternion.identity);
            if (obj.TryGetComponent(out QProjectile qp))
            {
                qp.Init(dir);
            }
            yield return new WaitForSeconds(qFireInterval);
        }
    }
    #endregion

    #region W Skill – Spin AoE
    private IEnumerator W_SpinAttack(bool weakened)
    {
        int repeats = SkillManager.Instance.IsUltimateActive ? wUltimateRepeats : 1;
        // 연속 사용에 따른 범위 패널티 계산
        float penaltyFactor = 1f;
        if (_wConsecutiveCount > 1)
        {
            penaltyFactor = Mathf.Max(wRadiusMin / wRadius, 1f - wRadiusPenaltyStep * (_wConsecutiveCount - 1));
        }

        float radius = wRadius * penaltyFactor;
        int damage = weakened ? wDamageWeakened : wDamage;

        for (int i = 0; i < repeats; i++)
        {
            DoSpinDamage(radius, damage);
            // 회전 이펙트 / 사운드 추가 위치
            yield return new WaitForSeconds(0.25f);
        }
    }

    private void DoSpinDamage(float radius, int damage)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var h in hits)
        {
            if (h.TryGetComponent(out Enemy enemy))
            {
                enemy.TakeDamage(damage);
            }

            // 적 투사체 삭제 (Tag 또는 컴포넌트 기반)
            if (h.CompareTag("EnemyBullet"))
            {
                Destroy(h.gameObject);
                continue;
            }
            // EnemyProjectile 컴포넌트(예: EnemyBullet.cs)가 있다면 제거
            if (h.TryGetComponent(out EnemyProjectile ep))
            {
                Destroy(ep.gameObject);
            }
        }

        // 범위 표시용 이펙트 생성
        if (wEffectPrefab != null)
        {
            GameObject fx = Instantiate(wEffectPrefab, transform.position, Quaternion.identity);

            // 이펙트가 범위(radius)에 맞춰 보이도록 스케일 조정 (가정: 프리팹 기본 크기 = 1단위)
            float scale = radius * 2f; // 지름 기준
            fx.transform.localScale = new Vector3(scale, scale, 1f);

            Destroy(fx, wEffectDuration);
        }
    }
    #endregion

    #region E Skill – Dash
    private void E_Dash()
    {
        float penaltyMultiplier = 1f;
        if (_eConsecutiveCount > 1)
        {
            penaltyMultiplier = Mathf.Max(eDashMinMultiplier, 1f - eDashPenaltyStep * (_eConsecutiveCount - 1));
        }

        Vector2 dashDir = _pc.CurrentInputDir;
        _pc.StartDash(dashDir, penaltyMultiplier);
    }
    #endregion

    #region R Skill – Barrage
    private void R_Barrage()
    {
        // 1) 벽에 갈고리 박힌 상태에서 재사용 -> 정확히 벽 지점까지 이동
        if (_hookLatchedWall)
        {
            Vector2 toWall = _wallPoint - (Vector2)transform.position;
            float distance = toWall.magnitude;
            Vector2 dir = distance > 0.01f ? toWall.normalized : Vector2.up;

            // dashSpeed * mult * dashDuration = distance  => mult = distance /(dashSpeed*duration)
            float mult = distance / (_pc.dashSpeed * _pc.dashDuration);
            mult = Mathf.Max(0.1f, mult); // safety
            _pc.StartDash(dir, mult);

            _hookLatchedWall = false;
            return;
        }

        // 2) 적에 갈고리가 붙어있는 상태에서 재사용 -> 돌진 공격
        if (_hookLatchedEnemy && _enemyTarget != null)
        {
            Vector2 toEnemy = (Vector2)_enemyTarget.transform.position - (Vector2)transform.position;
            float dist = toEnemy.magnitude;
            Vector2 dirDash = dist > 0.01f ? toEnemy.normalized : _lastHookDir;

            float mult = dist / (_pc.dashSpeed * _pc.dashDuration) * rEnemyDashExtraMultiplier;
            mult = Mathf.Max(0.2f, mult);

            _pc.StartDash(dirDash, mult);

            // 데미지를 대시 끝부분에서 적용
            StartCoroutine(R_DashHitEnemy(_enemyTarget));

            _hookLatchedEnemy = false;
            _enemyTarget = null;
            _lastHookDir = dirDash;
            return;
        }

        // 3) 이미 갈고리가 날아가는 중이면 재시전 무시
        if (_activeHook != null) return;

        // 4) 새로운 갈고리 발사
        if (rHookPrefab == null || _pc.firePoint == null) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dirShoot = (mouseWorld - _pc.firePoint.position).normalized;
        _lastHookDir = dirShoot;
        GameObject hookObj = Instantiate(rHookPrefab, _pc.firePoint.position, Quaternion.identity);
        if (hookObj.TryGetComponent(out HookProjectile hook))
        {
            hook.owner = this;
            hook.speed = rHookSpeed;
            hook.maxDistance = rHookMaxDistance;
            hook.Init(dirShoot);
            _activeHook = hook;
        }
    }

    private IEnumerator R_DashHitEnemy(Enemy target)
    {
        float delay = _pc.dashDuration * 0.9f;
        yield return new WaitForSeconds(delay);
        if (target != null)
        {
            target.TakeDamage(rCollisionDamage);

            // 반대 방향 임펄스
            Vector2 bounceDir = -_lastHookDir;
            if (bounceDir.sqrMagnitude < 0.01f) bounceDir = Vector2.up;
            _pc.Rigidbody2D.linearVelocity = Vector2.zero;
            _pc.AddImpulse(bounceDir * rBounceForce);
        }
    }

    // Callback from HookProjectile when hit enemy
    public void OnHookHitEnemy(Enemy enemy)
    {
        _activeHook = null;
        if (enemy == null) return;

        _hookLatchedEnemy = true;
        _enemyTarget = enemy;
    }

    // Callback when hook hits wall
    public void OnHookHitWall(Vector2 point)
    {
        _activeHook = null;
        _hookLatchedWall = true;
        _wallPoint = point;
        _hookLatchedEnemy = false;
        _enemyTarget = null;
    }
    #endregion
} 