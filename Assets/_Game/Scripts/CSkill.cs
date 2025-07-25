using System.Collections;
using UnityEngine;
using DarkTonic.MasterAudio;
using System.Collections.Generic; // Added for List

/// <summary>
/// C 스킬 – 대시 + 궤도 탄막 (기존 E 스킬).
/// weakened=true 일 때 연속 사용 패널티 적용(대시 거리 단축).
/// </summary>
public class CSkill : PlayerSkillBase
{
    [Header("대시 설정")]
    [Tooltip("연속 사용 시 대시 거리 감소 비율")] public float dashPenaltyStep = 0.2f;
    [Tooltip("대시 최소 거리 배수")] public float dashMinMultiplier = 0.4f;
    [Tooltip("대시 기본 속도 (단위/초)")] public float baseDashSpeed = 20f;

    [Header("실드 설정")]
    [Tooltip("대시 방향 앞을 막을 실드 프리팹")] public GameObject shieldPrefab;
    [Tooltip("실드가 생성될 거리")] public float shieldDistance = 1f;
    [Tooltip("실드 유지 시간")] public float shieldLifetime = 0.5f;

    [Header("실드 랭크별 설정")]
    [Tooltip("D, C, B, A, S 순으로 실드 개수 설정")] public int[] shieldCounts = {1, 3, 5, 7, 9};
    [Tooltip("D, C, B, A, S 순으로 실드 각도 설정")] public float[] shieldArcAngles = {40f, 70f, 100f, 130f, 160f};
    [Tooltip("D, C, B, A, S 순으로 실드가 적에게 입힐 데미지")] public int[] shieldDamagePerRank = {1, 2, 3, 4, 5};

    [Header("S 랭크 강화 설정")]
    [Tooltip("S 랭크 대시 속도 배수")] public float sDashSpeedMultiplier = 1.5f;
    [Tooltip("S 랭크 실드 지속 시간 배수")] public float sShieldLifetimeMultiplier = 1.5f;

    [Header("카메라 흔들림 설정")]
    [Tooltip("대시 시작 시 카메라 흔들림 지속 시간")] public float dashShakeDuration = 0.12f;
    [Tooltip("대시 시작 시 카메라 흔들림 강도")] public float dashShakeMagnitude = 0.18f;
    // bulletShake 변수 제거 (궤도 탄막 폐기)
    
    [Header("실드 피격 카메라 흔들림")]
    [Tooltip("실드가 적을 맞췄을 때 카메라 흔들림 지속 시간")] public float shieldHitShakeDuration = 0.08f;
    [Tooltip("실드가 적을 맞췄을 때 카메라 흔들림 강도")] public float shieldHitShakeMagnitude = 0.12f;
    
    [Header("실드 투사체 설정 (B랭크 이상)")]
    [Tooltip("실드가 날아갈 속도")] public float shieldProjectileSpeed = 14f;
    [Tooltip("A 랭크 이상에서 실드가 적에게 줄 넉백 힘")] public float shieldKnockbackForce = 8f;
    [Tooltip("A 랭크 이상에서 실드가 적을 밀어낼 거리")] public float shieldKnockbackDistance = 1.2f;
    
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

        // 랭크별 전방 실드 생성
        SpawnShieldObjects(dashDir, rank);
        yield break;
    }

    /// <summary>
    /// 랭크에 따라 전방 실드를 생성합니다.
    /// </summary>
    private void SpawnShieldObjects(Vector2 dashDir, StyleRank rank)
    {
        if (shieldPrefab == null) return;

        // 랭크별 개수 및 각도는 인스펙터에서 설정한 배열을 사용
        int idx = (int)rank;
        int count = (shieldCounts != null && shieldCounts.Length > idx) ? shieldCounts[idx] : 1;
        float arcDeg = (shieldArcAngles != null && shieldArcAngles.Length > idx) ? shieldArcAngles[idx] : 40f;
        int damageVal = (shieldDamagePerRank != null && shieldDamagePerRank.Length > idx) ? shieldDamagePerRank[idx] : 1;

        Vector2 centerDir = dashDir == Vector2.zero ? (Vector2)transform.up : dashDir.normalized;

        // A/S 랭크에서 딜레이 후 퍼질 실드 저장
        List<(Shield sh, Vector2 dir)> delayedSpread = new List<(Shield, Vector2)>();

        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0f : (float)i / (count - 1);
            float angleOffset = -arcDeg * 0.5f + t * arcDeg;
            float baseAngle = Mathf.Atan2(centerDir.y, centerDir.x) * Mathf.Rad2Deg;
            float finalAngle = baseAngle + angleOffset;
            Vector2 dir = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad));
            Vector2 offset = dir.normalized * shieldDistance;
            Vector2 spawnPos = (Vector2)transform.position + offset;

            GameObject obj = Instantiate(shieldPrefab, spawnPos, Quaternion.identity);
            if (obj.TryGetComponent(out Shield shield))
            {
                shield.lifetime = shieldLifetime;
                shield.target = transform; // 플레이어를 따라다님
                shield.localOffset = offset;
                shield.damage = damageVal;
                shield.shakeDuration = shieldHitShakeDuration;
                shield.shakeMagnitude = shieldHitShakeMagnitude;

                // B 랭크 이상: 실드 투사체로 날아감
                if (rank >= StyleRank.B)
                {
                    shield.followTarget = false;

                    // A랭크 이상: 처음엔 중심 방향, 나중에 퍼짐
                    if (rank >= StyleRank.A)
                    {
                        shield.moveDir = centerDir;
                        delayedSpread.Add((shield, dir.normalized));
                    }
                    else
                    {
                    shield.moveDir = centerDir;
                    }
                    shield.moveSpeed = shieldProjectileSpeed;

                    // S 랭크 – 실드 지속 시간 증가
                    if (rank == StyleRank.S)
                    {
                        shield.lifetime *= sShieldLifetimeMultiplier;
                    }
                }

                // A 랭크 이상: 넉백 효과 추가
                if (rank >= StyleRank.A)
                {
                    shield.applyKnockback = true;
                    shield.knockbackForce = shieldKnockbackForce;
                    shield.knockbackDistance = shieldKnockbackDistance;
                }
            }
            else
            {
                // 파괴 예약만 적용
                Destroy(obj, shieldLifetime);
            }
        }

        // 딜레이 후 퍼지 코루틴 시작
        if (rank >= StyleRank.A && delayedSpread.Count > 0)
        {
            StartCoroutine(SpreadShieldsAfterDelay(delayedSpread, pc.dashDuration));
        }
    }

    private IEnumerator SpreadShieldsAfterDelay(List<(Shield sh, Vector2 dir)> list, float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (var tup in list)
        {
            if (tup.sh != null)
            {
                tup.sh.moveDir = tup.dir;
            }
        }
    }

    // --- 궤도 탄막 관련 메서드 제거 완료 ---
} 