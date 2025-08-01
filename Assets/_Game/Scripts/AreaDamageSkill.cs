using UnityEngine;
using System.Linq; // LINQ를 사용하여 랭크 보너스를 쉽게 찾습니다.

/// <summary>
/// 특정 영역 내의 모든 적에게 피해를 주는 스킬 로직입니다.
/// </summary>
public class AreaDamageSkill : SkillBase
{
    public override void Activate(GameObject caster, SkillDataSO skillData, StyleRank currentRank)
    {
        // 1. 현재 랭크에 맞는 보너스 데이터 찾기
        var rankBonus = skillData.rankBonuses.FirstOrDefault(b => b.rank == currentRank);

        // 2. 랭크 보너스를 적용하여 최종 능력치 계산
        float finalRadius = skillData.baseRadius;
        float finalDamage = skillData.baseDamage;

        if (rankBonus != null)
        {
            finalRadius *= rankBonus.radiusMultiplier;
            finalDamage *= rankBonus.damageMultiplier;
        }

        // 3. 시각 효과(VFX) 생성
        // TODO: vfxPrefab 필드가 SkillDataSO에서 제거되었으므로, 필요하다면 MeleeAttackData 같은 곳에 추가해야 합니다.
        // if (skillData.meleeAttackData?.vfxPrefab != null) { ... }

        // 4. 공격 영역 내의 모든 적 감지 및 피해 적용
        Collider2D[] hits = Physics2D.OverlapCircleAll(caster.transform.position, finalRadius);

        Debug.Log($"[AreaDamageSkill] 랭크 {currentRank}: 반경 {finalRadius} 내의 {hits.Length}개 오브젝트 감지.");

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out EnemyHealth enemyHealth))
            {
                enemyHealth.TakeDamage((int)finalDamage);
                Debug.Log($"[AreaDamageSkill] {hit.name}에게 {finalDamage}의 데미지.");
            }
        }
    }
}
