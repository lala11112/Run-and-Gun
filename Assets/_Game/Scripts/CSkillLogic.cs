using UnityEngine;
using System.Linq;

/// <summary>
/// C 스킬(대시 & 실드)의 행동을 실제로 구현하는 전용 로직 클래스입니다.
/// </summary>
public class CSkillLogic : SkillBase
{
    [Header("C-Skill 고유 데이터")]
    [Tooltip("대시 행동에 대한 상세 데이터입니다.")]
    public DashData dashData;
    [Tooltip("실드 생성에 대한 상세 데이터입니다.")]
    public ShieldData shieldData;
    
    public override void Activate(GameObject caster, StyleRank currentRank)
    {
        var playerController = caster.GetComponent<PlayerController>();
        if (playerController == null) return;

        // 1. 대시 실행
        if(dashData != null)
        {
            playerController.Dash(playerController.LastMoveDir, dashData.dashSpeed, dashData.dashDuration);
            Debug.Log($"[CSkillLogic] 대시 실행! 속도: {dashData.dashSpeed}");
        }

        // 2. 실드 생성
        if (shieldData != null && shieldData.shieldPrefab != null)
        {
            SpawnShields(caster, currentRank);
        }
    }

    private void SpawnShields(GameObject caster, StyleRank currentRank)
    {
        var playerController = caster.GetComponent<PlayerController>();
        // 이제 항상 유효한 방향값을 가지는 LastMoveDir를 사용합니다.
        Vector2 centerDir = playerController.LastMoveDir; 

        // 랭크에 맞는 보너스 찾기 (현재는 하나의 보너스만 사용)
        var rankBonus = shieldData.shieldBonus;

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
