using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 주변의 적을 자동으로 탐색하여 지정된 횟수만큼 투사체를 연사하는 스킬 로직입니다.
/// </summary>
public class ChainedProjectileSkill : SkillBase
{
    private bool _isFiring;

    public override void Activate(GameObject caster, SkillDataSO skillData, StyleRank currentRank)
    {
        if (_isFiring) return;
        if (skillData.barrageData?.projectilePrefab == null) return;

        List<Transform> targets = FindTargets(caster, skillData.baseRadius);
        if (targets.Count == 0) return;

        StartCoroutine(FireRoutine(caster, skillData, currentRank, targets));
    }

    private List<Transform> FindTargets(GameObject caster, float radius)
    {
        List<Transform> validTargets = new List<Transform>();
        Collider2D[] hits = Physics2D.OverlapCircleAll(caster.transform.position, radius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<EnemyHealth>(out _) && !hit.CompareTag("Player"))
            {
                validTargets.Add(hit.transform);
            }
        }
        return validTargets;
    }

    private IEnumerator FireRoutine(GameObject caster, SkillDataSO skillData, StyleRank currentRank, List<Transform> targets)
    {
        _isFiring = true;

        Transform firePoint = caster.transform.Find("FirePoint");
        if (firePoint == null)
        {
            _isFiring = false;
            yield break;
        }

        // 1. 랭크 보너스 적용
        var rankBonus = skillData.rankBonuses.FirstOrDefault(b => b.rank == currentRank);
        float finalDamage = skillData.baseDamage;
        int finalCount = skillData.baseCount;
        
        if (rankBonus != null)
        {
            finalDamage *= rankBonus.damageMultiplier;
            finalCount = Mathf.RoundToInt(skillData.baseCount * rankBonus.countMultiplier);
        }
        
        // 2. 연사 실행
        for (int i = 0; i < finalCount; i++)
        {
            Transform currentTarget = targets[i % targets.Count]; // 순환하며 타겟팅
            if (currentTarget == null) continue;

            GameObject projectileInstance = Instantiate(
                skillData.barrageData.projectilePrefab,
                firePoint.position,
                Quaternion.identity
            );

            if (projectileInstance.TryGetComponent<Projectile>(out var projectileComponent))
            {
                Vector2 direction = (currentTarget.position - firePoint.position).normalized;
                // projectileComponent.speed = skillData.barrageData.projectileSpeed;
                projectileComponent.damage = (int)finalDamage;
                projectileComponent.Init(direction);
            }

            if (skillData.barrageData.fireInterval > 0)
            {
                yield return new WaitForSeconds(skillData.barrageData.fireInterval);
            }
        }

        _isFiring = false;
    }
}
