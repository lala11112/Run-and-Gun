using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkTonic.MasterAudio;
using System.Linq;
// SimplePool 사용

/// <summary>
/// Z 스킬 – 기존 Q 스킬 기능 (적에게 탄막 연사 + 이동속도 버프)
/// </summary>
public class ZSkill : PlayerSkillBase
{
    [Tooltip("발사할 투사체 프리팹")] public GameObject projectilePrefab;
    [Tooltip("투사체 발사 횟수")] public int projectileCount = 8;
    [Tooltip("투사체 간 발사 간격(초)")] public float fireInterval = 0.07f;

    [Header("속도 버프 설정")] [Tooltip("이동 속도 배수")] public float speedMultiplier = 1.8f;
    [Tooltip("속도 버프 지속 시간")] public float speedDuration = 3f;
    [Tooltip("적 탐색 최대 반경")] public float targetSearchRadius = 20f;

    [Header("시야 판정 설정")] [Tooltip("시야를 차단하는 장애물 레이어 마스크")] public LayerMask obstacleLayers;

    [Header("트레일 설정")] [Tooltip("속도 버프 중 생성될 트레일 프리팹")] public GameObject trailPrefab;
    [Tooltip("트레일 유지 시간")] public float trailLifetime = 1.5f;
    [Tooltip("같은 위치 중복 생성을 막는 최소 거리")] public float trailMinDistance = 0.3f;

    [Header("카메라 흔들림 프리셋")] 
    [Tooltip("투사체 발사 시 사용할 Shake 프리셋 이름")] public string shootShakePreset = "EnemyHit";

    [Header("스타일 비용 설정")]
    [Tooltip("스킬 사용 시 소비될 스타일 점수")] public int styleCost = 30;

    [Header("S 랭크 강화 설정")]
    [Tooltip("S 랭크에서 투사체 발사 횟수 배수")] public float sProjectileCountMultiplier = 2f;
    [Tooltip("S 랭크에서 이동 속도 버프 배수")] public float sSpeedMultiplierMultiplier = 1.3f;
    [Tooltip("S 랭크에서 투사체 간 발사 간격 배수 (0~1, 작을수록 빠름)")] public float sFireIntervalMultiplier = 0.5f;

    [Tooltip("D 랭크에서 투사체 발사 횟수 배수 (0~1, 작을수록 적음)")] public float dProjectileCountMultiplier = 0.5f;

    private int _speedBuffCount = 0;
    private float _baseMoveSpeed;
    private Vector2 _lastTrailPos;

    // 사운드 중복 방지를 위한 정적 타임스탬프
    private static float _lastShootSfxTime;
    private static float _lastShootShakeTime;
    private const float ShootSfxMinInterval = 0.02f; // 최소 간격 (초)

    protected override void Awake()
    {
        base.Awake();
        skillType = SkillType.Z;
        _baseMoveSpeed = pc.moveSpeed;
    }

    protected override IEnumerator Activate(bool weakened)
    {
        // 현재 랭크를 소비 전 상태로 저장
        StyleRank rank = StyleManager.Instance != null ? StyleManager.Instance.CurrentRank : StyleRank.C;

        // 스타일 점수 소모 (소모 후 랭크가 변해도 rank 변수는 유지)
        if (StyleManager.Instance != null)
        {
            StyleManager.Instance.ConsumeScore(styleCost);
        }

        // S 랭크 강화 파라미터 계산
        int projCount = projectileCount;
        float fireInt = fireInterval;
        float moveSpeedMult = speedMultiplier;

        if (rank == StyleRank.S)
        {
            projCount = Mathf.RoundToInt(projectileCount * sProjectileCountMultiplier);
            fireInt = fireInterval * sFireIntervalMultiplier;
            moveSpeedMult = speedMultiplier * sSpeedMultiplierMultiplier;
        }
        else if (rank == StyleRank.D)
        {
            projCount = Mathf.Max(1, Mathf.RoundToInt(projectileCount * dProjectileCountMultiplier));
        }

        // 대상 선정
        List<Transform> targets = new List<Transform>();
        Vector2 originPos = pc.firePoint != null ? (Vector2)pc.firePoint.position : (Vector2)transform.position;

        Collider2D[] nearHits = Physics2D.OverlapCircleAll(transform.position, targetSearchRadius);

        if (rank == StyleRank.C || rank == StyleRank.D)
        {
            float minDist = float.MaxValue;
            Transform closest = null;
            foreach (var h in nearHits)
            {
                Transform t = null;
                if (h.TryGetComponent(out SimpleEnemy enemy))
                {
                    t = enemy.transform;
                }
                else if (h.TryGetComponent(out IDamageable dmg))
                {
                    // 플레이어 자신 제외
                    if (h.CompareTag("Player")) continue;
                    t = h.transform;
                }

                if (t != null && HasLineOfSight(originPos, t.position))
                {
                    float d = Vector2.Distance(transform.position, t.position);
                    if (d < minDist)
                    {
                        minDist = d;
                        closest = t;
                    }
                }
            }
            if (closest != null) targets.Add(closest);
        }
        else // B, A, S – 범위 내 모든 대상
        {
            foreach (var h in nearHits)
            {
                Transform t = null;
                if (h.TryGetComponent(out SimpleEnemy enemy))
                {
                    t = enemy.transform;
                }
                else if (h.TryGetComponent(out IDamageable dmg))
                {
                    if (h.CompareTag("Player")) continue;
                    t = h.transform;
                }

                if (t != null && HasLineOfSight(originPos, t.position))
                    targets.Add(t);
            }
        }

        if (targets.Count > 0 && projectilePrefab != null && pc.firePoint != null)
        {
            foreach (var t in targets)
            {
                StartCoroutine(FireBarrage(t, projCount, fireInt));
            }
        }

        // 속도 버프 & 트레일
        _speedBuffCount++;
        if (_speedBuffCount == 1)
        {
            pc.moveSpeed = _baseMoveSpeed * moveSpeedMult;
            if (rank == StyleRank.B || rank == StyleRank.A || rank == StyleRank.S) // C, D는 트레일 없음
                StartCoroutine(TrailCoroutine(speedDuration));
        }

        yield return new WaitForSeconds(speedDuration);

        _speedBuffCount--;
        if (_speedBuffCount <= 0)
        {
            _speedBuffCount = 0;
            pc.moveSpeed = _baseMoveSpeed;
        }
    }

    private IEnumerator FireBarrage(Transform target, int count, float interval)
    {
        for (int i = 0; i < count; i++)
        {
            if (target == null) break;
            Vector2 dir = ((Vector2)target.position - (Vector2)pc.firePoint.position).normalized;
            GameObject obj = SimplePool.Spawn(projectilePrefab, pc.firePoint.position, Quaternion.identity);
            if (obj.TryGetComponent(out QProjectile qp)) qp.Init(dir);

            // 효과음 (중복 방지)
            if (Time.time - _lastShootSfxTime > ShootSfxMinInterval)
            {
                MasterAudio.PlaySound3DAtTransform("ZSkillShoot", pc.firePoint != null ? pc.firePoint : transform);
                _lastShootSfxTime = Time.time;
            }

            // 카메라 흔들림 (중복 방지)
            if (CameraManager.Instance != null && Time.time - _lastShootShakeTime > ShootSfxMinInterval)
            {
                CameraManager.Instance.ShakeWithPreset(shootShakePreset);
                _lastShootShakeTime = Time.time;
            }
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator TrailCoroutine(float duration)
    {
        // 새 트레일 시퀀스 초기화

        // 첫 지점 즉시 생성
        Vector2 startPos = transform.position;
        SpawnTrail(startPos);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector2 currentPos = transform.position;
            if (Vector2.Distance(currentPos, _lastTrailPos) >= trailMinDistance)
            {
                SpawnTrail(currentPos);
            }
            yield return null;
        }
        // 위치 기록 갱신
    }

    private void SpawnTrail(Vector2 pos)
    {
        GameObject zone = Instantiate(trailPrefab, pos, Quaternion.identity);
        if (zone.TryGetComponent(out QTrailZone qtz)) qtz.lifetime = trailLifetime;
        _lastTrailPos = pos;
    }

    /// <summary>
    /// 플레이어(또는 발사 지점)와 대상 사이에 장애물이 있는지 검출하여 시야 여부를 판단합니다.
    /// </summary>
    /// <param name="origin">레이 시작점 (주로 firePoint)</param>
    /// <param name="targetPos">대상 위치</param>
    /// <returns>true = 시야 확보, false = 시야 차단</returns>
    private bool HasLineOfSight(Vector2 origin, Vector2 targetPos)
    {
        Vector2 dir = targetPos - origin;
        float dist = dir.magnitude;
        if (dist <= 0.01f) return true;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir.normalized, dist, obstacleLayers);

        // hit.collider가 null이면 사이에 장애물이 없음 → 시야 O
        return hit.collider == null;
    }
} 