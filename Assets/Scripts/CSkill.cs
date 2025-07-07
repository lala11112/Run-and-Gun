using System.Collections;
using UnityEngine;
using DarkTonic.MasterAudio;

/// <summary>
/// C 스킬 – 대시 + 궤도 탄막 (기존 E 스킬).
/// weakened=true 일 때 연속 사용 패널티 적용(대시 거리 단축).
/// </summary>
public class CSkill : PlayerSkillBase
{
    [Header("대시 설정")] [Tooltip("연속 사용 시 대시 거리 감소 비율")] public float dashPenaltyStep = 0.2f;
    [Tooltip("대시 최소 거리 배수")] public float dashMinMultiplier = 0.4f;

    [Header("궤도 탄막 설정")] [Tooltip("대시 시 생성될 궤도 탄막 프리팹")] public GameObject orbitPrefab;
    [Tooltip("궤도 반경")] public float orbitRadius = 1.2f;
    [Tooltip("궤도 각속도(°/s)")] public float orbitAngularSpeedDeg = 360f;
    [Tooltip("대시 종료 후 궤도 유지 시간")] public float orbitExtraDuration = 0.3f;
    [Tooltip("궤도 탄막 데미지")] public int orbitDamage = 3;

    [Header("S 랭크 강화 설정")]
    [Tooltip("S 랭크 대시 속도 배수")] public float sDashSpeedMultiplier = 1.5f;
    [Tooltip("S 랭크 궤도 반경 배수")] public float sOrbitRadiusMultiplier = 1.3f;
    [Tooltip("S 랭크 궤도 탄막 데미지 배수")] public float sOrbitDamageMultiplier = 1.5f;

    [Header("대시 속도")] [Tooltip("대시 기본 속도 (단위/초)")] public float baseDashSpeed = 20f;

    [Header("카메라 흔들림 설정")] 
    [Tooltip("대시 시작 시 카메라 흔들림 지속 시간")] public float dashShakeDuration = 0.12f;
    [Tooltip("대시 시작 시 카메라 흔들림 강도")] public float dashShakeMagnitude = 0.18f;
    [Tooltip("궤도 탄막 적중 시 카메라 흔들림 지속 시간")] public float bulletShakeDuration = 0.08f;
    [Tooltip("궤도 탄막 적중 시 흔들림 강도")] public float bulletShakeMagnitude = 0.12f;

    // 내부 상태: 이번 스킬 사용 중 탄막 shake 이미 플레이됐는지
    internal bool BulletShakePlayed { get; private set; }
    
    private float _baseMoveSpeed;

    protected override void Awake()
    {
        base.Awake();
        skillType = SkillType.C;
        _baseMoveSpeed = pc.moveSpeed;
    }

    protected override IEnumerator Activate(bool weakened)
    {
        StyleRank rank = StyleManager.Instance != null ? StyleManager.Instance.CurrentRank : StyleRank.C;

        float penaltyMultiplier = weakened ? Mathf.Max(dashMinMultiplier, 1f - dashPenaltyStep) : 1f;

        // 현재 이동 속도에 따른 추가 배수 (기본 속도 대비 비율)
        float speedScale = pc.moveSpeed / _baseMoveSpeed;

        float finalSpeed = baseDashSpeed * penaltyMultiplier * speedScale;

        if (rank == StyleRank.S)
        {
            finalSpeed *= sDashSpeedMultiplier;
        }

        Vector2 dashDir = pc.CurrentInputDir;
        pc.StartDash(dashDir, finalSpeed);

        // 대시 사운드 & 카메라 흔들림
        MasterAudio.PlaySound3DAtTransform("Dash", transform);
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(dashShakeDuration, dashShakeMagnitude);
        }

        // 궤도 탄막 파라미터 계산
        float rad = orbitRadius;
        int dmg = orbitDamage;
        if (rank == StyleRank.S)
        {
            rad *= sOrbitRadiusMultiplier;
            dmg = Mathf.RoundToInt(orbitDamage * sOrbitDamageMultiplier);
        }

        BulletShakePlayed = false; // 이번 스킬에서는 아직 흔들림 미발생
        SpawnOrbitBullets(rad, dmg);
        yield break;
    }

    private void SpawnOrbitBullets(float radius, int dmg)
    {
        if (orbitPrefab == null) return;

        float totalDur = pc.dashDuration + orbitExtraDuration;
        for (int i = 0; i < 3; i++)
        {
            float angleDeg = i * 120f;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius;
            GameObject obj = Instantiate(orbitPrefab, (Vector2)transform.position + offset, Quaternion.identity);
            if (obj.TryGetComponent(out DashOrbitBullet orb))
            {
                orb.center = transform;
                orb.radius = radius;
                orb.angularSpeedDeg = orbitAngularSpeedDeg;
                orb.damage = dmg;
                orb.lifetime = totalDur;
                orb.startAngleDeg = angleDeg;
                orb.parentSkill = this; // 한 번만 흔들리도록 레퍼런스 전달
            }
        }
    }

    // 외부에서 첫 흔들림 시 호출
    internal void MarkBulletShake() => BulletShakePlayed = true;

    // BulletShakeDuration, BulletShakeMagnitude 프로퍼티는 그대로
    public float BulletShakeDuration => bulletShakeDuration;
    public float BulletShakeMagnitude => bulletShakeMagnitude;
} 