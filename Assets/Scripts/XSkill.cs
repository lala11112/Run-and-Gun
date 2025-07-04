using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// X 스킬 – 주변 스핀/폭발 공격 (기존 W 스킬)
/// 랭크별: C=기본, B=반복+밀쳐내기, A=범위증가+반사
/// </summary>
public class XSkill : PlayerSkillBase
{
    [Header("기본 설정")]
    [Tooltip("기본 공격 범위 반경")] public float radius = 3f;
    [Tooltip("기본 데미지 (C 랭크, 첫 타)")] public int damage = 2;
    [Tooltip("연속 사용 패널티 시 데미지")] public int damageWeakened = 1;

    [Header("연속 사용 패널티 설정")]
    [Tooltip("연속 사용 시 반경 감소 비율 (0~1)")] public float radiusPenaltyStep = 0.2f;
    [Tooltip("최소 반경 한계값")] public float radiusMin = 0.5f;

    [Header("랭크 강화 설정")]
    [Tooltip("B/A 랭크에서 스핀 반복 횟수")] public int rankedRepeats = 3;
    [Tooltip("밀쳐내기 힘 (Impulse)")] public float knockbackForce = 8f;
    [Tooltip("A 랭크 범위 배수")] public float aRankRadiusMultiplier = 1.8f;
    [Tooltip("밀쳐내기 부드러운 이동 지속 시간")] public float knockbackDuration = 0.15f;

    [Header("내부 범위 설정")]
    [Tooltip("내부 범위 비율 (radius * ratio)")] public float innerRadiusRatio = 0.5f;
    [Tooltip("내부 범위 추가 데미지")] public int innerBonusDamage = 2;
    [Tooltip("내부 범위 스턴 지속 시간")] public float innerStunDuration = 1f;

    [Header("S 랭크 강화 설정")]
    [Tooltip("S 랭크에서 반경 배수")] public float sRadiusMultiplier = 2f;
    [Tooltip("S 랭크에서 데미지 배수")] public float sDamageMultiplier = 1.5f;

    [Header("이펙트 설정")]
    [Tooltip("외부 범위 이펙트 프리팹")] public GameObject outerEffectPrefab;
    [Tooltip("내부 범위 이펙트 프리팹")] public GameObject innerEffectPrefab;
    [Tooltip("범위 이펙트 지속 시간")] public float effectDuration = 0.3f;

    protected override void Awake()
    {
        base.Awake();
        skillType = SkillType.X;
    }

    protected override IEnumerator Activate(bool weakened)
    {
        StyleRank rank = StyleManager.Instance != null ? StyleManager.Instance.CurrentRank : StyleRank.C;

        int repeats = rankedRepeats;
        if (rank == StyleRank.C) repeats = 1;

        float r = radius;
        if (rank == StyleRank.A) r *= aRankRadiusMultiplier;

        // S 랭크 강화 적용
        if (rank == StyleRank.S)
        {
            r *= sRadiusMultiplier;
        }

        int baseDmg = weakened ? damageWeakened : damage;
        if (rank == StyleRank.S)
        {
            baseDmg = Mathf.RoundToInt(baseDmg * sDamageMultiplier);
        }

        for (int i = 0; i < repeats; i++)
        {
            DoSpin(r, baseDmg, rank);
            yield return new WaitForSeconds(0.25f);
        }
    }

    private void DoSpin(float rad, int dmg, StyleRank rank)
    {
        float innerRad = rad * innerRadiusRatio;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, rad);
        foreach (var h in hits)
        {
            if (h.TryGetComponent(out Enemy enemy))
            {
                int dealt = dmg;
                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist <= innerRad)
                {
                    dealt += innerBonusDamage;
                    enemy.Stun(innerStunDuration);
                }
                enemy.TakeDamage(dealt);

                // Knockback
                if (rank != StyleRank.C && enemy.TryGetComponent(out Rigidbody2D erb))
                {
                    Vector2 dir = (enemy.transform.position - transform.position);
                    float curDist = dir.magnitude;
                    if (curDist < rad)
                    {
                        Vector2 target = (Vector2)enemy.transform.position + dir.normalized * (rad - curDist + 0.1f);
                        StartCoroutine(KnockbackSmooth(erb, target, knockbackDuration));
                    }
                }
            }

            // Projectile interaction
            if (h.CompareTag("EnemyBullet"))
            {
                if (rank == StyleRank.A) ReflectProjectile(h.gameObject, h.transform.position);
                else Destroy(h.gameObject);
            }
            else if (h.TryGetComponent(out EnemyProjectile ep))
            {
                if (rank == StyleRank.A) ReflectProjectile(ep.gameObject, ep.transform.position);
                else Destroy(ep.gameObject);
            }
        }

        // Effects
        if (outerEffectPrefab != null)
        {
            var fx = Object.Instantiate(outerEffectPrefab, transform.position, Quaternion.identity);
            fx.transform.localScale = Vector3.one * rad * 2f;
            Object.Destroy(fx, effectDuration);
        }
        if (innerEffectPrefab != null)
        {
            var fx2 = Object.Instantiate(innerEffectPrefab, transform.position, Quaternion.identity);
            fx2.transform.localScale = Vector3.one * innerRad * 2f;
            Object.Destroy(fx2, effectDuration);
        }

        StyleManager.Instance?.RegisterSkillHit(SkillType.X);
    }

    private void ReflectProjectile(GameObject enemyProj, Vector3 pos)
    {
        if (pc.projectilePrefab == null) { Object.Destroy(enemyProj); return; }
        Vector2 dir = (pos - transform.position).normalized;
        var obj = Object.Instantiate(pc.projectilePrefab, pos, Quaternion.identity);
        if (obj.TryGetComponent(out Projectile p)) p.Init(dir);
        Object.Destroy(enemyProj);
    }

    private IEnumerator KnockbackSmooth(Rigidbody2D rb, Vector2 target, float dur)
    {
        Vector2 start = rb.position;
        float elapsed = 0f;
        while (elapsed < dur && rb != null)
        {
            elapsed += Time.deltaTime;
            rb.MovePosition(Vector2.Lerp(start, target, elapsed / dur));
            yield return null;
        }

        if (rb != null)
        {
            rb.MovePosition(target);
        }
    }
} 