using System.Collections;
using UnityEngine;
using System.Collections.Generic;

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

    [Header("R Skill – Giant Shot Settings")]
    [Tooltip("R 스킬 거대 투사체 프리팹")] public GameObject rGiantProjectilePrefab;
    [Tooltip("거대 투사체 속도")] public float rGiantProjectileSpeed = 15f;
    [Tooltip("거대 투사체 데미지")] public int rGiantProjectileDamage = 3;

    [Header("W Rank Up Settings")]
    [Tooltip("B/A 랭크에서 스핀 폭발 반복 횟수")] public int wRankedRepeats = 3;
    [Tooltip("B/A 랭크에서 적 밀쳐내기 임펄스 힘")] public float wKnockbackForce = 8f;
    [Tooltip("A 랭크 범위 배율")] public float wARankRadiusMultiplier = 1.8f;
    [Tooltip("밀쳐내기 지속 시간(초)")] public float wKnockbackDuration = 0.15f;

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
                R_GiantShot();
                break;
        }
    }

    #region Q Skill – Stun + Speed Buff
    private IEnumerator Q_StunSkill(bool weakened)
    {
        StyleRank rank = StyleManager.Instance != null ? StyleManager.Instance.CurrentRank : StyleRank.C;

        List<Enemy> targets = new List<Enemy>();

        if (rank == StyleRank.C)
        {
            // 가장 가까운 적 하나 찾기
            Collider2D[] nearHits = Physics2D.OverlapCircleAll(transform.position, qTargetSearchRadius);
            float minDist = float.MaxValue;
            Enemy closest = null;
            foreach (var h in nearHits)
            {
                if (h.TryGetComponent(out Enemy enemy))
                {
                    float d = Vector2.Distance(transform.position, enemy.transform.position);
                    if (d < minDist)
                    {
                        minDist = d;
                        closest = enemy;
                    }
                }
            }
            if (closest != null) targets.Add(closest);
        }
        else // B, A 랭크 – 범위 내 모든 적
        {
            Collider2D[] nearHits = Physics2D.OverlapCircleAll(transform.position, qTargetSearchRadius);
            foreach (var h in nearHits)
            {
                if (h.TryGetComponent(out Enemy enemy))
                {
                    targets.Add(enemy);
                }
            }
        }

        if (targets.Count > 0 && qProjectilePrefab != null && _pc.firePoint != null)
        {
            foreach (var t in targets)
            {
                StartCoroutine(Q_FireBarrage(t));
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
        StyleRank rank = StyleManager.Instance != null ? StyleManager.Instance.CurrentRank : StyleRank.C;

        int repeats = SkillManager.Instance.IsUltimateActive ? wUltimateRepeats : 1;

        // 랭크별 추가 폭발 횟수
        if (rank == StyleRank.B || rank == StyleRank.A)
            repeats = wRankedRepeats;

        // 연속 사용에 따른 범위 패널티 계산
        float penaltyFactor = 1f;
        if (_wConsecutiveCount > 1)
        {
            penaltyFactor = Mathf.Max(wRadiusMin / wRadius, 1f - wRadiusPenaltyStep * (_wConsecutiveCount - 1));
        }

        float radius = wRadius * penaltyFactor;
        int damage = weakened ? wDamageWeakened : wDamage;

        // A 랭크 시 범위 증가
        if (rank == StyleRank.A)
        {
            radius *= wARankRadiusMultiplier;
        }

        for (int i = 0; i < repeats; i++)
        {
            DoSpinDamage(radius, damage, rank);
            // 회전 이펙트 / 사운드 추가 위치
            yield return new WaitForSeconds(0.25f);
        }
    }

    private void DoSpinDamage(float radius, int damage, StyleRank rank)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        bool hitEnemy = false;
        foreach (var h in hits)
        {
            if (h.TryGetComponent(out Enemy enemy))
            {
                enemy.TakeDamage(damage);
                hitEnemy = true;

                // B, A 랭크에서 밀쳐내기 구현 (폭발력)
                if (rank != StyleRank.C && enemy.TryGetComponent(out Rigidbody2D erigidbody))
                {
                    Vector2 toEnemy = enemy.transform.position - transform.position;
                    float dist = toEnemy.magnitude;
                    if (dist < radius)
                    {
                        Vector2 pushDir = toEnemy.normalized;
                        float pushDistance = radius - dist + 0.1f; // 패딩
                        Vector2 targetPos = (Vector2)enemy.transform.position + pushDir * pushDistance;
                        StartCoroutine(KnockbackSmooth(erigidbody, targetPos, wKnockbackDuration));
                    }
                }
            }

            if (h.CompareTag("EnemyBullet"))
            {
                if (rank == StyleRank.A)
                {
                    ReflectProjectile(h.gameObject, h.transform.position);
                }
                else
            {
                Destroy(h.gameObject);
                }
                continue;
            }
            // EnemyProjectile 컴포넌트가 있을 때
            if (h.TryGetComponent(out EnemyProjectile ep))
            {
                if (rank == StyleRank.A)
                {
                    ReflectProjectile(ep.gameObject, ep.transform.position);
                }
                else
            {
                Destroy(ep.gameObject);
                }
            }
        }

        if (hitEnemy)
        {
            StyleManager.Instance?.RegisterSkillHit(SkillType.W);
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

    // 적 투사체를 반사하여 플레이어 방향 기준 밖으로 되돌려보냄
    private void ReflectProjectile(GameObject enemyProj, Vector3 projPos)
    {
        if (_pc.projectilePrefab == null || _pc.firePoint == null) { Destroy(enemyProj); return; }

        Vector2 dir = (projPos - transform.position).normalized; // 밖으로 향함

        GameObject proj = Instantiate(_pc.projectilePrefab, projPos, Quaternion.identity);
        if (proj.TryGetComponent(out Projectile p))
        {
            p.Init(dir);
        }

        Destroy(enemyProj);
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

    #region R Skill – Giant Shot
    private void R_GiantShot()
        {
        if (rGiantProjectilePrefab == null || _pc.firePoint == null) return;

        // 네 방향: 앞, 뒤, 좌, 우 (플레이어 기준)
        Vector2[] dirs = new Vector2[]
        {
            _pc.transform.up,
            -_pc.transform.up,
            _pc.transform.right,
            -_pc.transform.right
        };

        foreach (var dir in dirs)
        {
            Quaternion rot = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
            GameObject obj = Instantiate(rGiantProjectilePrefab, _pc.firePoint.position, rot);
            if (obj.TryGetComponent(out GiantProjectile gp))
        {
                gp.speed = rGiantProjectileSpeed;
                gp.damage = rGiantProjectileDamage;
                gp.Init(dir);
        }
    }

        StyleManager.Instance?.RegisterSkillHit(SkillType.R);
    }
    #endregion

    // 부드러운 넉백 코루틴
    private IEnumerator KnockbackSmooth(Rigidbody2D rb, Vector2 targetPos, float duration)
    {
        Vector2 start = rb.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Vector2 newPos = Vector2.Lerp(start, targetPos, t);
            rb.MovePosition(newPos);
            yield return null;
        }
        rb.MovePosition(targetPos);
    }
} 