using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Z 스킬(연사 + 자기 버프)의 복잡한 행동을 실제로 구현하는 전용 로직 클래스입니다.
/// </summary>
public class ZSkillLogic : SkillBase
{
    private bool _isFiring; // 연사 중복 실행 방지
    
    public override void Activate(GameObject caster, SkillDataSO skillData, StyleRank currentRank)
    {
        // 자기 버프 적용
        if (skillData.selfBuffData != null)
        {
            ApplySelfBuff(caster, skillData.selfBuffData, currentRank);
        }

        // 투사체 연사
        if (skillData.barrageData != null && !_isFiring)
        {
            List<Transform> targets = FindTargets(caster, skillData.baseRadius);
            if(targets.Count > 0)
            {
                StartCoroutine(FireRoutine(caster, skillData, currentRank, targets));
            }
        }
    }

    private void ApplySelfBuff(GameObject caster, SelfBuffData buffData, StyleRank currentRank)
    {
        var playerController = caster.GetComponent<PlayerController>();
        if (playerController == null) return;
        
        // TODO: PlayerController에 버프를 관리하는 로직이 필요합니다.
        // 예: playerController.ApplySpeedBuff(buffData.moveSpeedModifier, buffData.buffDuration);
        
        Debug.Log($"[ZSkillLogic] {caster.name}에게 {buffData.buffDuration}초 동안 이동 속도 버프 적용!");
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
        var rankBonus = skillData.rankBonuses.FirstOrDefault(b => b.rank == currentRank) ?? new RankBonusData();
        float finalDamage = skillData.baseDamage * rankBonus.damageMultiplier;
        int finalCount = Mathf.RoundToInt(skillData.baseCount * rankBonus.countMultiplier);

        // 2. 연사 실행
        for (int i = 0; i < finalCount; i++)
        {
            Transform currentTarget = targets[i % targets.Count];
            if (currentTarget == null) continue;

            GameObject projectileInstance = Instantiate(
                skillData.barrageData.projectilePrefab,
                firePoint.position,
                Quaternion.identity
            );

            if (projectileInstance.TryGetComponent<Projectile>(out var projectileComponent))
            {
                Vector2 direction = (currentTarget.position - firePoint.position).normalized;
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
