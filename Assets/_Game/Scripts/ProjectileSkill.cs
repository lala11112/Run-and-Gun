using UnityEngine;
using System.Linq;

/// <summary>
/// '투사체 발사'라는 행동을 책임지는 구체적인 스킬 로직 클래스입니다.
/// </summary>
public class ProjectileSkill : SkillBase
{
    public override void Activate(GameObject caster, SkillDataSO skillData, StyleRank currentRank)
    {
        // 1. 데이터 유효성 검사
        if (skillData.barrageData?.projectilePrefab == null)
        {
            Debug.LogError($"[ProjectileSkill] Skill '{skillData.skillName}'의 projectilePrefab이 비어있습니다!");
            return;
        }

        Transform firePoint = caster.transform.Find("FirePoint");
        if (firePoint == null)
        {
            Debug.LogError($"[ProjectileSkill] Caster '{caster.name}'에서 'FirePoint'를 찾을 수 없습니다.");
            return;
        }

        // 2. 랭크 보너스 적용
        var rankBonus = skillData.rankBonuses.FirstOrDefault(b => b.rank == currentRank);
        float finalDamage = skillData.baseDamage;
        if (rankBonus != null)
        {
            finalDamage *= rankBonus.damageMultiplier;
        }

        // 3. 투사체 생성 및 초기화
        GameObject projectileInstance = Instantiate(
            skillData.barrageData.projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );

        if (projectileInstance.TryGetComponent<Projectile>(out var projectileComponent))
        {
            // Projectile 컴포넌트에 직접 값을 설정합니다.
            // projectileComponent.speed = skillData.barrageData.projectileSpeed; // barrageData에 speed 추가 필요
            projectileComponent.damage = (int)finalDamage;
            projectileComponent.Init(firePoint.up);
        }

        Debug.Log($"'{caster.name}'이(가) 스킬 '{skillData.skillName}' 발동! (랭크: {currentRank}, 최종 데미지: {finalDamage})");
    }
}
