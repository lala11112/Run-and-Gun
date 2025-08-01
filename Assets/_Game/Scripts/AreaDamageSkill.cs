using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// 특정 영역 내의 모든 적에게 피해를 주는 스킬 로직입니다.
/// 이 스크립트가 붙은 프리팹의 인스펙터에서 직접 데미지와 범위를 설정할 수 있습니다.
/// </summary>
public class AreaDamageSkill : SkillBase
{
    /// <summary>
    /// AreaDamageSkill의 랭크별 보너스 데이터만 담는 전용 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class AreaDamageRankBonus
    {
        public StyleRank rank;
        [Tooltip("데미지에 곱해질 배율입니다.")]
        public float damageMultiplier = 1f;
        [Tooltip("공격 범위에 곱해질 배율입니다.")]
        public float radiusMultiplier = 1f;
    }

    [Header("광역 공격 데이터")]
    [Tooltip("공격의 기본 데미지입니다.")]
    public float damage = 15f;
    [Tooltip("공격의 기본 반경입니다.")]
    public float radius = 5f;
    [Tooltip("공격 시 생성될 이펙트 프리팹입니다.")]
    public GameObject hitEffectPrefab;
    [Tooltip("AreaDamage 스킬의 랭크별 성능 변화 목록입니다.")]
    public List<AreaDamageRankBonus> rankBonuses = new List<AreaDamageRankBonus>();

    public override void Activate(GameObject caster, StyleRank currentRank)
    {
        // 1. 현재 랭크에 맞는 보너스 데이터 찾기
        var rankBonus = rankBonuses.FirstOrDefault(b => b.rank == currentRank) ?? new AreaDamageRankBonus();

        // 2. 랭크 보너스를 적용하여 최종 능력치 계산
        float finalRadius = radius * rankBonus.radiusMultiplier;
        float finalDamage = damage * rankBonus.damageMultiplier;

        // 3. 시각 효과(VFX) 생성
        if (hitEffectPrefab != null)
        {
            // TODO: XSkillLogic처럼 이펙트 크기도 조절하는 로직 추가 가능
            Instantiate(hitEffectPrefab, caster.transform.position, Quaternion.identity);
        }

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
