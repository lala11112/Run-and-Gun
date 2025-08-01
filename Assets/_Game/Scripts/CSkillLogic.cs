using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// C 스킬(대시 & 실드)의 행동을 실제로 구현하는 전용 로직 클래스입니다.
/// </summary>
public class CSkillLogic : SkillBase
{
    public override void Activate(GameObject caster, SkillDataSO skillData, StyleRank currentRank)
    {
        var playerController = caster.GetComponent<PlayerController>();
        if (playerController == null) return;

        // 1. 대시 실행
        if (skillData.dashData != null)
        {
            // TODO: PlayerController에 대시 기능 구현 필요
            // playerController.Dash(playerController.CurrentInputDir, skillData.dashData.dashSpeed, skillData.dashData.dashDuration);
            Debug.Log($"[CSkillLogic] 대시 실행! 속도: {skillData.dashData.dashSpeed}");
        }

        // 2. 실드 생성
        if (skillData.shieldData != null && skillData.shieldData.shieldPrefab != null)
        {
            SpawnShields(caster, skillData.shieldData, currentRank);
        }
    }

    private void SpawnShields(GameObject caster, ShieldData shieldData, StyleRank currentRank)
    {
        var playerController = caster.GetComponent<PlayerController>();
        Vector2 centerDir = playerController.CurrentInputDir != Vector2.zero ? playerController.CurrentInputDir : (Vector2)caster.transform.up;

        // 랭크에 맞는 보너스 찾기
        var rankBonus = shieldData.shieldBonus; // 현재는 하나의 보너스만 사용

        for (int i = 0; i < rankBonus.count; i++)
        {
            float angleOffset = (rankBonus.count > 1) ? (-rankBonus.arcAngle * 0.5f + i * (rankBonus.arcAngle / (rankBonus.count - 1))) : 0;
            float finalAngle = Mathf.Atan2(centerDir.y, centerDir.x) * Mathf.Rad2Deg + angleOffset;
            
            Vector2 dir = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad));
            Vector2 spawnPos = (Vector2)caster.transform.position + dir * 1.0f; // 실드 생성 거리

            GameObject shieldInstance = Instantiate(shieldData.shieldPrefab, spawnPos, Quaternion.Euler(0, 0, finalAngle - 90f));
            
            if(shieldInstance.TryGetComponent<Shield>(out var shieldComponent))
            {
                // TODO: Shield.cs에 랭크별 데이터 적용 로직 필요
                // shieldComponent.Initialize(rankBonus.damage, rankBonus.lifetime, rankBonus.applyKnockback);
            }
        }
        Debug.Log($"[CSkillLogic] {rankBonus.count}개의 실드 생성!");
    }
}
