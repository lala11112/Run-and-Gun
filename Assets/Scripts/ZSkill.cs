using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("트레일 설정")] [Tooltip("속도 버프 중 생성될 트레일 프리팹")] public GameObject trailPrefab;
    [Tooltip("트레일 유지 시간")] public float trailLifetime = 1.5f;
    [Tooltip("같은 위치 중복 생성을 막는 최소 거리")] public float trailMinDistance = 0.3f;

    [Header("Style Cost")]
    [Tooltip("스킬 사용 시 소비될 스타일 점수")] public int styleCost = 30;

    private int _speedBuffCount = 0;
    private float _baseMoveSpeed;
    private Vector2 _lastTrailPos;
    private bool _hasLastTrailPos;

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

        // 대상 선정
        List<Enemy> targets = new List<Enemy>();
        if (rank == StyleRank.C)
        {
            Collider2D[] nearHits = Physics2D.OverlapCircleAll(transform.position, targetSearchRadius);
            float minDist = float.MaxValue;
            Enemy closest = null;
            foreach (var h in nearHits)
            {
                if (h.TryGetComponent(out Enemy enemy))
                {
                    float d = Vector2.Distance(transform.position, enemy.transform.position);
                    if (d < minDist)
                    {
                        minDist = d;
                        closest = enemy;
                    }
                }
            }
            if (closest != null) targets.Add(closest);
        }
        else // B, A
        {
            Collider2D[] nearHits = Physics2D.OverlapCircleAll(transform.position, targetSearchRadius);
            foreach (var h in nearHits)
            {
                if (h.TryGetComponent(out Enemy enemy)) targets.Add(enemy);
            }
        }

        if (targets.Count > 0 && projectilePrefab != null && pc.firePoint != null)
        {
            foreach (var t in targets)
            {
                StartCoroutine(FireBarrage(t));
            }
        }

        // 속도 버프 & 트레일
        _speedBuffCount++;
        if (_speedBuffCount == 1)
        {
            pc.moveSpeed = _baseMoveSpeed * speedMultiplier;
            if (rank != StyleRank.C && trailPrefab != null)
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

    private IEnumerator FireBarrage(Enemy target)
    {
        for (int i = 0; i < projectileCount; i++)
        {
            if (target == null) break;
            Vector2 dir = (target.transform.position - pc.firePoint.position).normalized;
            GameObject obj = Instantiate(projectilePrefab, pc.firePoint.position, Quaternion.identity);
            if (obj.TryGetComponent(out QProjectile qp)) qp.Init(dir);
            yield return new WaitForSeconds(fireInterval);
        }
    }

    private IEnumerator TrailCoroutine(float duration)
    {
        _hasLastTrailPos = false; // 새 트레일 시퀀스 초기화

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
        _hasLastTrailPos = false;
    }

    private void SpawnTrail(Vector2 pos)
    {
        GameObject zone = Instantiate(trailPrefab, pos, Quaternion.identity);
        if (zone.TryGetComponent(out QTrailZone qtz)) qtz.lifetime = trailLifetime;
        _lastTrailPos = pos;
        _hasLastTrailPos = true;
    }
} 