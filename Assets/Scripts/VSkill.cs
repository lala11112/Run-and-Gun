using System.Collections;
using UnityEngine;

/// <summary>
/// V 스킬 – 네 방향 거대 투사체 발사 (기존 R 스킬)
/// </summary>
public class VSkill : PlayerSkillBase
{
    [Header("거대 투사체 설정")] [Tooltip("발사할 거대 투사체 프리팹")] public GameObject giantProjectilePrefab;
    [Tooltip("투사체 속도")] public float projectileSpeed = 15f;
    [Tooltip("투사체 데미지")] public int projectileDamage = 3;

    protected override void Awake()
    {
        base.Awake();
        skillType = SkillType.V;
    }

    protected override IEnumerator Activate(bool weakened)
    {
        if (giantProjectilePrefab == null || pc.firePoint == null) yield break;

        Vector2[] dirs = new Vector2[]
        {
            pc.transform.up,
            -pc.transform.up,
            pc.transform.right,
            -pc.transform.right
        };

        foreach (var dir in dirs)
        {
            Quaternion rot = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
            GameObject obj = Instantiate(giantProjectilePrefab, pc.firePoint.position, rot);
            if (obj.TryGetComponent(out GiantProjectile gp))
            {
                gp.speed = projectileSpeed;
                gp.damage = projectileDamage;
                gp.Init(dir);
            }
        }

        StyleManager.Instance?.RegisterSkillHit(SkillType.V);
        yield break;
    }
} 