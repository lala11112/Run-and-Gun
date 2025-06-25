using System.Collections;
using UnityEngine;

/// <summary>
/// C 스킬 – 대시 + 궤도 탄막 (기존 E 스킬).
/// weakened=true 일 때 연속 사용 패널티 적용(대시 거리 단축).
/// </summary>
public class CSkill : PlayerSkillBase
{
    [Header("Dash Settings")] [Tooltip("연속 사용 시 대시 거리 감소 비율")] public float dashPenaltyStep = 0.2f;
    [Tooltip("대시 최소 거리 배수")] public float dashMinMultiplier = 0.4f;

    [Header("Orbit Bullet Settings")] [Tooltip("대시 시 생성될 궤도 탄막 프리팹")] public GameObject orbitPrefab;
    [Tooltip("궤도 반경")] public float orbitRadius = 1.2f;
    [Tooltip("궤도 각속도(°/s)")] public float orbitAngularSpeedDeg = 360f;
    [Tooltip("대시 종료 후 궤도 유지 시간")] public float orbitExtraDuration = 0.3f;
    [Tooltip("궤도 탄막 데미지")] public int orbitDamage = 3;

    [Header("Dash Speed")] [Tooltip("대시 기본 속도 (단위/초)")] public float baseDashSpeed = 20f;

    private float _baseMoveSpeed;

    protected override void Awake()
    {
        base.Awake();
        skillType = SkillType.C;
        _baseMoveSpeed = pc.moveSpeed;
    }

    protected override IEnumerator Activate(bool weakened)
    {
        float penaltyMultiplier = weakened ? Mathf.Max(dashMinMultiplier, 1f - dashPenaltyStep) : 1f;

        // 현재 이동 속도에 따른 추가 배수 (기본 속도 대비 비율)
        float speedScale = pc.moveSpeed / _baseMoveSpeed;

        float finalSpeed = baseDashSpeed * penaltyMultiplier * speedScale;

        Vector2 dashDir = pc.CurrentInputDir;
        pc.StartDash(dashDir, finalSpeed);

        SpawnOrbitBullets();
        yield break;
    }

    private void SpawnOrbitBullets()
    {
        if (orbitPrefab == null) return;

        float totalDur = pc.dashDuration + orbitExtraDuration;
        for (int i = 0; i < 3; i++)
        {
            float angleDeg = i * 120f;
            float rad = angleDeg * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * orbitRadius;
            GameObject obj = Instantiate(orbitPrefab, (Vector2)transform.position + offset, Quaternion.identity);
            if (obj.TryGetComponent(out DashOrbitBullet orb))
            {
                orb.center = transform;
                orb.radius = orbitRadius;
                orb.angularSpeedDeg = orbitAngularSpeedDeg;
                orb.damage = orbitDamage;
                orb.lifetime = totalDur;
                orb.startAngleDeg = angleDeg;
            }
        }
    }
} 