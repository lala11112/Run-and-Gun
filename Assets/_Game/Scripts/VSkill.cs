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

    [Header("투사체 라이프타임")]
    [Tooltip("거대 투사체가 존재할 최대 시간(초)")] public float projectileLifetime = 2.5f;

    [Header("곡선 궤도 설정")]
    [Tooltip("A 랭크 이상일 때 투사체가 궤도를 휘며 날아갈 회전 속도(도/초)")]
    public float curveDegreesPerSecond = 120f;

    [Header("점프/내리치기 설정")]
    [Tooltip("점프 힘(임펄스)")] public float jumpForce = 8f;
    [Tooltip("공중 체공 시간(초)")] public float airTime = 0.4f;
    [Tooltip("땅 충돌 시 카메라 흔들림 지속 시간")] public float slamShakeDuration = 0.2f;
    [Tooltip("땅 충돌 시 카메라 흔들림 세기")] public float slamShakeMagnitude = 0.3f;

    [Header("파티클 설정")] [Tooltip("투사체 궤도에 생성할 파티클 프리팹")] public GameObject trailParticlePrefab;
    [Tooltip("궤도당 파티클 개수")] public int particlesPerPath = 6;
    [Tooltip("파티클 간 거리")] public float particleSpacing = 1f;

    private bool _isRunning;

    protected override void Awake()
    {
        base.Awake();
        skillType = SkillType.V;
    }

    protected override IEnumerator Activate(bool weakened)
    {
        if (_isRunning || giantProjectilePrefab == null || pc.firePoint == null) yield break;
        _isRunning = true;

        // 1) 점프 (임펄스) + 무적
        var cols = pc.GetComponentsInChildren<Collider2D>(true);
        foreach (var c in cols) c.enabled = false;

        if (pc.Rigidbody2D != null)
        {
            pc.Rigidbody2D.linearVelocity = Vector2.zero;
            pc.Rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(airTime);

        // 2) 내리치기 – 카메라 흔들림
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(slamShakeDuration, slamShakeMagnitude);
        }

        // 플레이어 제자리 고정
        if (pc.Rigidbody2D != null) pc.Rigidbody2D.linearVelocity = Vector2.zero;

        // 충돌 다시 활성
        foreach (var c in cols) c.enabled = true;

        // 3) 실제 V 스킬 공격 발동
        yield return StartCoroutine(SpawnProjectilesAndEffects());

        _isRunning = false;
    }

    private IEnumerator SpawnProjectilesAndEffects()
    {
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

        List<Vector2> dirList = new() { pc.transform.up, -pc.transform.up, pc.transform.right, -pc.transform.right };
        if (rank >= StyleRank.B)
        {
            dirList.Add((pc.transform.up + pc.transform.right).normalized);
            dirList.Add((pc.transform.up - pc.transform.right).normalized);
            dirList.Add((-pc.transform.up + pc.transform.right).normalized);
            dirList.Add((-pc.transform.up - pc.transform.right).normalized);
        }

        Vector2[] dirs = dirList.ToArray();

        MasterAudio.PlaySound3DAtTransform("Shoots", pc.firePoint != null ? pc.firePoint : pc.transform);

        foreach (var dir in dirs)
        {
            Quaternion rot = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
            GameObject obj = SimplePool.Spawn(giantProjectilePrefab, pc.firePoint.position, rot);
            if (obj.TryGetComponent(out GiantProjectile gp))
            {
                gp.speed = speed;
                gp.damage = dmg;
                gp.lifetime = projectileLifetime;
                gp.Init(dir);

                if (rank == StyleRank.A)
                {
                    gp.stunOnHit = true;
                    gp.stunDuration = aStunDuration;
                    gp.curved = true;
                    gp.curveDegreesPerSecond = GetCurveSign(dir) * curveDegreesPerSecond;
                }
                else if (rank == StyleRank.S)
                {
                    gp.pullOnHit = true;
                    gp.pullForce = sPullForce;
                    gp.curved = true;
                    gp.curveDegreesPerSecond = GetCurveSign(dir) * curveDegreesPerSecond;
                }
            }

            obj.transform.localScale *= scaleMult;

            // 파티클 연출 : A 랭크 이상은 투사체 궤도를 따라 실시간 생성
            if (trailParticlePrefab != null)
            {
                if (rank >= StyleRank.A)
                {
                    StartCoroutine(SpawnTrailAlongProjectile(obj.transform, speed));
                }
                else
                {
                    StartCoroutine(SpawnTrailParticles(dir, particlesPerPath));
                }
            }
        }

        StyleManager.Instance?.RegisterSkillHit(SkillType.V);
        yield break;
    }

    private IEnumerator SpawnTrailParticles(Vector2 dir, int count)
    {
        Vector3 start = pc.firePoint.position;
        Vector3 step = (Vector3)dir.normalized * particleSpacing;
        for (int i = 1; i <= count; i++)
        {
            Vector3 pos = start + step * i;
            Instantiate(trailParticlePrefab, pos, Quaternion.identity);
            yield return new WaitForSeconds(0.04f);
        }
    }

    // 투사체를 따라가며 파티클을 생성 (곡선 궤도 대응)
    private IEnumerator SpawnTrailAlongProjectile(Transform proj, float _)
    {
        if (proj == null) yield break;

        Vector3 lastPos = proj.position;
        int spawned = 0;
        while (proj != null && spawned < particlesPerPath)
        {
            float dist = Vector3.Distance(lastPos, proj.position);
            if (dist >= particleSpacing)
            {
                Instantiate(trailParticlePrefab, proj.position, Quaternion.identity);
                lastPos = proj.position;
                spawned++;
            }
            yield return null; // 다음 프레임까지 대기
        }
    }

    // helper method
    private float GetCurveSign(Vector2 dir)
    {
        // 모든 투사체를 시계 방향(양의 각도)으로 회전시킵니다.
        return 1f;
    }
} 