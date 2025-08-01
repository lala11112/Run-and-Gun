using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// '투사체 발사'라는 행동을 책임지는 구체적인 스킬 로직 클래스입니다.
/// 이 스크립트가 붙은 프리팹의 인스펙터에서 투사체의 모든 세부 사항을 직접 설정할 수 있습니다.
/// </summary>
public class ProjectileSkill : SkillBase
{
    /// <summary>
    /// ProjectileSkill의 랭크별 보너스 데이터만 담는 전용 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class ProjectileRankBonus
    {
        public StyleRank rank;
        [Tooltip("데미지에 곱해질 배율입니다.")]
        public float damageMultiplier = 1f;
    }

    [Header("투사체 공격 데이터")]
    [Tooltip("발사할 투사체 프리팹입니다. Projectile 컴포넌트가 있어야 합니다.")]
    public Projectile projectilePrefab;
    [Tooltip("투사체의 기본 데미지입니다.")]
    public float damage = 10f;
    [Tooltip("투사체의 속도입니다.")]
    public float speed = 20f;
    
    [Header("랭크별 성장 정보")]
    [Tooltip("Projectile 스킬의 랭크별 성능 변화 목록입니다.")]
    public List<ProjectileRankBonus> rankBonuses = new List<ProjectileRankBonus>();

    public override void Activate(GameObject caster, StyleRank currentRank)
    {
        // 1. 데이터 유효성 검사
        if (projectilePrefab == null)
        {
            Debug.LogError($"[ProjectileSkill] projectilePrefab이 비어있습니다!");
            return;
        }

        Transform firePoint = caster.transform.Find("FirePoint");
        if (firePoint == null)
        {
            Debug.LogError($"[ProjectileSkill] Caster '{caster.name}'에서 'FirePoint'를 찾을 수 없습니다.");
            return;
        }

        // 2. 랭크 보너스 적용
        var rankBonus = rankBonuses.FirstOrDefault(b => b.rank == currentRank) ?? new ProjectileRankBonus();
        float finalDamage = damage * rankBonus.damageMultiplier;

        // 3. 투사체 생성 및 초기화
        Projectile projectileInstance = Instantiate(
            projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );

        // Projectile 컴포넌트에 직접 값을 설정합니다.
        projectileInstance.speed = speed;
        projectileInstance.damage = (int)finalDamage;
        projectileInstance.Init(firePoint.up);

        Debug.Log($"'{caster.name}'이(가) ProjectileSkill 발동! (랭크: {currentRank}, 최종 데미지: {finalDamage})");
    }
}
