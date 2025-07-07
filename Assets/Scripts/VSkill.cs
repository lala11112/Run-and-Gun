using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using DarkTonic.MasterAudio;

/// <summary>
/// V 스킬 – 네 방향 거대 투사체 발사 (기존 R 스킬)
/// </summary>
public class VSkill : PlayerSkillBase
{
    [Header("거대 투사체 설정")] [Tooltip("발사할 거대 투사체 프리팹")] public GameObject giantProjectilePrefab;
    [Tooltip("투사체 속도")] public float projectileSpeed = 15f;
    [Tooltip("투사체 데미지")] public int projectileDamage = 3;

    [Header("S 랭크 강화 설정")]
    [Tooltip("S 랭크에서 투사체 속도 배수")] public float sProjectileSpeedMultiplier = 1.5f;
    [Tooltip("S 랭크에서 투사체 데미지 배수")] public float sProjectileDamageMultiplier = 2f;
    [Tooltip("S 랭크에서 투사체 크기 배수")] public float sProjectileScaleMultiplier = 1.2f;

    [Header("랭크별 추가 효과 설정")] 
    [Tooltip("A 랭크 기절 지속 시간")] public float aStunDuration = 1f;
    [Tooltip("S 랭크 끌어당김 힘")] public float sPullForce = 12f;

    protected override void Awake()
    {
        base.Awake();
        skillType = SkillType.V;
    }

    protected override IEnumerator Activate(bool weakened)
    {
        if (giantProjectilePrefab == null || pc.firePoint == null) yield break;

        StyleRank rank = StyleManager.Instance != null ? StyleManager.Instance.CurrentRank : StyleRank.C;

        float speed = projectileSpeed;
        int dmg = projectileDamage;
        float scaleMult = 1f;

        if (rank == StyleRank.S)
        {
            speed *= sProjectileSpeedMultiplier;
            dmg = Mathf.RoundToInt(projectileDamage * sProjectileDamageMultiplier);
            scaleMult = sProjectileScaleMultiplier;
        }

        // 랭크에 따른 방향 배열 (B 랭크 이상 8방향, 그 외 4방향)
        List<Vector2> dirList = new List<Vector2> { pc.transform.up, -pc.transform.up, pc.transform.right, -pc.transform.right };
        // 발동 시 1회 Shoot 사운드 재생 (MasterAudio Sound Group 이름은 "Shoots" 로 가정)
        MasterAudio.PlaySound3DAtTransform("Shoots", pc.firePoint != null ? pc.firePoint : pc.transform);
        if (rank >= StyleRank.B)
        {
            dirList.Add((pc.transform.up + pc.transform.right).normalized);
            dirList.Add((pc.transform.up - pc.transform.right).normalized);
            dirList.Add((-pc.transform.up + pc.transform.right).normalized);
            dirList.Add((-pc.transform.up - pc.transform.right).normalized);
        }

        Vector2[] dirs = dirList.ToArray();

        foreach (var dir in dirs)
        {
            Quaternion rot = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
            GameObject obj = Instantiate(giantProjectilePrefab, pc.firePoint.position, rot);
            if (obj.TryGetComponent(out GiantProjectile gp))
            {
                gp.speed = speed;
                gp.damage = dmg;
                gp.Init(dir);

                // 랭크별 추가 효과 적용
                if (rank == StyleRank.A)
                {
                    gp.stunOnHit = true;
                    gp.stunDuration = aStunDuration;
                }
                else if (rank == StyleRank.S)
                {
                    gp.pullOnHit = true;
                    gp.pullForce = sPullForce;
                }
            }

            // 투사체 크기 조절
            obj.transform.localScale *= scaleMult;
        }

        StyleManager.Instance?.RegisterSkillHit(SkillType.V);
        yield break;
    }
} 