using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio; // MasterAudio를 사용하므로 네임스페이스 추가

/// <summary>
/// X 스킬(스핀 공격)의 복잡한 행동을 실제로 구현하는 전용 로직 클래스입니다.
/// </summary>
public class XSkillLogic : SkillBase
{
    public override void Activate(GameObject caster, SkillDataSO skillData, StyleRank currentRank)
    {
        StartCoroutine(SpinRoutine(caster, skillData, currentRank));
    }

    private IEnumerator SpinRoutine(GameObject caster, SkillDataSO skillData, StyleRank currentRank)
    {
        // 1. 현재 랭크에 맞는 보너스 데이터 찾기
        var rankBonus = skillData.rankBonuses.FirstOrDefault(b => b.rank == currentRank) ?? new RankBonusData();

        // 2. 랭크 보너스를 적용하여 최종 능력치 계산
        float finalRadius = skillData.baseRadius * rankBonus.radiusMultiplier;
        float finalDamage = skillData.baseDamage * rankBonus.damageMultiplier;
        int repeats = Mathf.RoundToInt(skillData.baseCount * rankBonus.countMultiplier);

        // 3. 반복 공격 실행
        for (int i = 0; i < repeats; i++)
        {
            PerformSpin(caster, skillData, finalRadius, finalDamage, rankBonus);
            if (i < repeats - 1)
            {
                yield return new WaitForSeconds(0.25f); // 반복 공격 사이의 딜레이
            }
        }
    }

    private void PerformSpin(GameObject caster, SkillDataSO skillData, float radius, float damage, RankBonusData rankBonus)
    {
        // 내부/외부 범위 계산
        float innerRadius = radius * (skillData.meleeAttackData?.innerRadiusRatio ?? 0.5f);
        bool innerHit = false;
        bool outerHit = false;

        // 공격 영역 내의 모든 적 감지
        Collider2D[] hits = Physics2D.OverlapCircleAll(caster.transform.position, radius);
        foreach (var hit in hits)
        {
            // 적에게 데미지 처리
            if (hit.TryGetComponent(out EnemyHealth enemyHealth))
            {
                float dealtDamage = damage;
                float dist = Vector2.Distance(caster.transform.position, hit.transform.position);

                // 내부 범위 판정
                if (dist <= innerRadius)
                {
                    dealtDamage += skillData.meleeAttackData?.innerBonusDamage ?? 0;
                    // TODO: Stun 로직 추가 (enemyHealth에 Stun 함수 필요)
                    // enemyHealth.Stun(skillData.meleeAttackData.innerStunDuration);
                    innerHit = true;
                }
                else
                {
                    outerHit = true;
                }
                enemyHealth.TakeDamage((int)dealtDamage);

                // 넉백 판정
                if (rankBonus.enableKnockback && hit.TryGetComponent(out Rigidbody2D enemyRb))
                {
                    Vector2 knockbackDir = (hit.transform.position - caster.transform.position).normalized;
                    enemyRb.AddForce(knockbackDir * 10f, ForceMode2D.Impulse); // 임시 넉백, 기존 로직으로 교체 필요
                }
            }

            // 투사체 반사 판정
            if (rankBonus.enableProjectileReflection && hit.CompareTag("EnemyBullet"))
            {
                // TODO: 투사체 반사 로직 구현
                Destroy(hit.gameObject);
            }
        }
        
        // --- 사운드 및 이펙트 처리 ---
        // MasterAudio.PlaySound3DAtTransform(innerHit ? "InSwords" : "Swords", caster.transform);
        // CameraManager.Instance?.ShakeWithPreset(innerHit ? "Skill_SpinHit" : "EnemyHit");
        // TODO: VFX 생성 로직 추가
    }
}
