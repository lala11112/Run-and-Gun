using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Z 스킬(연사 + 자기 버프)의 복잡한 행동을 실제로 구현하는 전용 로직 클래스입니다.
/// </summary>
public class ZSkillLogic : SkillBase
{
    /// <summary>
    /// Z스킬의 랭크별 보너스 데이터만 담는 전용 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class ZRankBonus
    {
        public StyleRank rank;
        [Tooltip("데미지에 곱해질 배율입니다.")]
        public float damageMultiplier = 1f;
        [Tooltip("발사 횟수에 곱해질 배율입니다.")]
        public float countMultiplier = 1f;
        [Tooltip("이동 속도 버프량에 곱해질 배율입니다.")]
        public float moveSpeedBuffMultiplier = 1f;
    }

    [Header("Z-Skill 고유 데이터")]
    [Tooltip("투사체 연사에 대한 상세 데이터입니다.")]
    public BarrageData barrageData;
    [Tooltip("시전자 자신에게 거는 버프에 대한 상세 데이터입니다.")]
    public SelfBuffData selfBuffData;
    [Tooltip("타겟을 탐지할 반경입니다.")]
    public float targettingRadius = 10f;
    [Tooltip("투사체의 기본 데미지입니다.")]
    public float baseDamage = 5f;

    [Header("랭크별 성장 정보")]
    [Tooltip("Z스킬의 랭크별 성능 변화 목록입니다.")]
    public List<ZRankBonus> rankBonuses = new List<ZRankBonus>();
    
    private bool _isFiring; // 연사 중복 실행 방지
    
    public override void Activate(GameObject caster, StyleRank currentRank)
    {
        if (selfBuffData != null) ApplySelfBuff(caster, currentRank);
        if (barrageData != null && !_isFiring)
        {
            List<Transform> targets = FindTargets(caster, targettingRadius);
            if(targets.Count > 0) StartCoroutine(FireRoutine(caster, currentRank, targets));
        }
    }

    private void ApplySelfBuff(GameObject caster, StyleRank currentRank)
    {
        var playerController = caster.GetComponent<PlayerController>();
        if (playerController == null) return;

        var rankBonus = rankBonuses.FirstOrDefault(b => b.rank == currentRank) ?? new ZRankBonus();
        float finalSpeedModifier = selfBuffData.baseMoveSpeedModifier * rankBonus.moveSpeedBuffMultiplier;
        
        playerController.ApplySpeedBuff(finalSpeedModifier, selfBuffData.buffDuration);
        Debug.Log($"[ZSkillLogic] {caster.name}에게 {selfBuffData.buffDuration}초 동안 {finalSpeedModifier * 100}%의 이동 속도 버프 적용! (랭크: {currentRank})");
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
    
    private IEnumerator FireRoutine(GameObject caster, StyleRank currentRank, List<Transform> targets)
    {
        _isFiring = true;

        Transform firePoint = caster.transform.Find("FirePoint");
        if (firePoint == null)
        {
            _isFiring = false;
            yield break;
        }

        var rankBonus = rankBonuses.FirstOrDefault(b => b.rank == currentRank) ?? new ZRankBonus();
        float finalDamage = baseDamage * rankBonus.damageMultiplier;
        int finalCount = Mathf.RoundToInt(barrageData.projectileCount * rankBonus.countMultiplier);

        for (int i = 0; i < finalCount; i++)
        {
            if (i >= targets.Count) continue; // 타겟 수를 초과하여 발사하지 않도록 방지
            Transform currentTarget = targets[i % targets.Count];
            if (currentTarget == null) continue;

            GameObject projectileInstance = Instantiate(barrageData.projectilePrefab, firePoint.position, Quaternion.identity);

            if (projectileInstance.TryGetComponent<Projectile>(out var projectileComponent))
            {
                Vector2 direction = (currentTarget.position - firePoint.position).normalized;
                projectileComponent.damage = (int)finalDamage;
                projectileComponent.speed = barrageData.projectileSpeed;
                projectileComponent.Init(direction);
            }

            if (barrageData.fireInterval > 0)
            {
                yield return new WaitForSeconds(barrageData.fireInterval);
            }
        }

        _isFiring = false;
    }
}
