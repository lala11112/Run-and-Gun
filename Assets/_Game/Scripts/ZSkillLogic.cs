using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Z 스킬(연사 + 자기 버프)의 복잡한 행동을 실제로 구현하는 전용 로직 클래스입니다.
/// </summary>
public class ZSkillLogic : SkillBase
{
    /// <summary>
    /// Z스킬의 랭크별 보너스 데이터만 담는 전용 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class ZRankBonus
    {
        public StyleRank rank;
        [Tooltip("데미지에 곱해질 배율입니다.")]
        public float damageMultiplier = 1f;
        [Tooltip("발사 횟수에 곱해질 배율입니다.")]
        public float countMultiplier = 1f;
        [Tooltip("이동 속도 버프량에 곱해질 배율입니다.")]
        public float moveSpeedBuffMultiplier = 1f;
    }

    [Header("Z-Skill 고유 데이터")]
    [Tooltip("투사체 연사에 대한 상세 데이터입니다.")]
    public BarrageData barrageData;
    [Tooltip("시전자 자신에게 거는 버프에 대한 상세 데이터입니다.")]
    public SelfBuffData selfBuffData;
    [Tooltip("타겟을 탐지할 반경입니다.")]
    public float targettingRadius = 10f;
    [Tooltip("투사체의 기본 데미지입니다.")]
    public float baseDamage = 5f;

    [Header("랭크별 성장 정보")]
    [Tooltip("Z스킬의 랭크별 성능 변화 목록입니다.")]
    public List<ZRankBonus> rankBonuses = new List<ZRankBonus>();
    
    [Header("게임 설정")]
    [Tooltip("게임 전체 설정이 담긴 ScriptableObject입니다.")]
    public GameConfigSO gameConfig;
    
    private bool _isFiring; // 연사 중복 실행 방지
    
    // 성능 최적화: 컴포넌트 캐싱
    private PlayerController _cachedPlayerController;
    private Transform _cachedFirePoint;
    
    // 성능 최적화: Physics 쿼리 결과 재사용을 위한 배열
    private Collider2D[] _targetSearchResults;
    
    // 메모리 누수 방지: 진행 중인 코루틴 추적
    private Coroutine _fireCoroutine;
    
    private void Awake()
    {
        // 성능 최적화: 배열 크기를 GameConfig에서 가져오거나 기본값 사용
        int arraySize = gameConfig != null ? gameConfig.maxTargetCount : 16;
        _targetSearchResults = new Collider2D[arraySize];
    }
    
    public override void Activate(GameObject caster, StyleRank currentRank)
    {
        // 컴포넌트 캐싱 (처음 호출시에만)
        if (_cachedPlayerController == null)
        {
            _cachedPlayerController = caster.GetComponent<PlayerController>();
        }
        if (_cachedFirePoint == null)
        {
            string firePointName = gameConfig != null ? gameConfig.firePointName : "FirePoint";
            _cachedFirePoint = caster.transform.Find(firePointName);
            if (_cachedFirePoint == null)
            {
                Debug.LogError($"[ZSkillLogic] '{caster.name}' 오브젝트에서 '{firePointName}'를 찾을 수 없습니다.");
                return;
            }
        }

        if (selfBuffData != null) ApplySelfBuff(currentRank);
        if (barrageData != null && !_isFiring)
        {
            List<Transform> targets = FindTargets(caster, targettingRadius);
            if (targets.Count > 0)
            {
                // 이전 코루틴이 실행 중이면 중지
                if (_fireCoroutine != null)
                {
                    StopCoroutine(_fireCoroutine);
                }
                _fireCoroutine = StartCoroutine(FireRoutine(currentRank, targets));
            }
            else
            {
                Debug.LogWarning("[ZSkillLogic] 발사할 타겟을 찾지 못했습니다. 주변에 적이 있는지, 적 프리팹 구조(EnemyHealth 위치)를 확인해주세요.");
            }
        }
    }

    private void ApplySelfBuff(StyleRank currentRank)
    {
        if (_cachedPlayerController == null) return;

        var rankBonus = rankBonuses.FirstOrDefault(b => b.rank == currentRank) ?? new ZRankBonus();
        float finalSpeedModifier = selfBuffData.baseMoveSpeedModifier * rankBonus.moveSpeedBuffMultiplier;
        
        _cachedPlayerController.ApplySpeedBuff(finalSpeedModifier, selfBuffData.buffDuration);
    }

    /// <summary>
    /// 지정된 반경 내에서 유효한 모든 타겟을 찾습니다.
    /// GetComponentInParent를 사용하여 콜라이더와 EnemyHealth 스크립트가 다른 오브젝트에 있어도 안정적으로 타겟을 탐지합니다.
    /// </summary>
    private List<Transform> FindTargets(GameObject caster, float radius)
    {
        List<Transform> validTargets = new List<Transform>();
        
        // 성능 최적화: 배열 재사용으로 GC 압박 감소
        int hitCount = Physics2D.OverlapCircleNonAlloc(caster.transform.position, radius, _targetSearchResults);
        
        for (int i = 0; i < hitCount; i++)
        {
            var hit = _targetSearchResults[i];
            if (hit == null) continue;
            
            // 부모 오브젝트에서 EnemyHealth 컴포넌트를 찾습니다.
            var enemyHealth = hit.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // 이미 리스트에 추가된 적인지 확인하여 중복 추가를 방지합니다.
                if (!validTargets.Contains(enemyHealth.transform))
                {
                    validTargets.Add(enemyHealth.transform);
                }
            }
        }
        return validTargets;
    }
    
    private IEnumerator FireRoutine(StyleRank currentRank, List<Transform> targets)
    {
        _isFiring = true;

        var rankBonus = rankBonuses.FirstOrDefault(b => b.rank == currentRank) ?? new ZRankBonus();
        float finalDamage = baseDamage * rankBonus.damageMultiplier;
        int finalCount = Mathf.RoundToInt(barrageData.projectileCount * rankBonus.countMultiplier);

        for (int i = 0; i < finalCount; i++)
        {
            if (targets.Count == 0) continue; 

            Transform currentTarget = targets[i % targets.Count];
            if (currentTarget == null) continue;

            // Object Pool 사용: Instantiate 대신 풀에서 가져오기
            GameObject projectileInstance = AdvancedObjectPool.Spawn(
                barrageData.projectilePrefab, 
                _cachedFirePoint.position, 
                Quaternion.identity
            );

            if (projectileInstance.TryGetComponent<QProjectile>(out var projectileComponent))
            {
                Vector2 direction = (currentTarget.position - _cachedFirePoint.position).normalized;
                projectileComponent.Init(
                    direction, 
                    (int)finalDamage, 
                    barrageData.projectileSpeed, 
                    barrageData.projectileLifetime
                );
            }
            else
            {
                 Debug.LogError($"[ZSkillLogic] 발사하려는 투사체 '{barrageData.projectilePrefab.name}'에 QProjectile 컴포넌트가 없습니다!", barrageData.projectilePrefab);
                 // Object Pool 사용: Destroy 대신 풀로 반환
                 AdvancedObjectPool.Despawn(projectileInstance);
            }

            if (barrageData.fireInterval > 0)
            {
                yield return new WaitForSeconds(barrageData.fireInterval);
            }
        }

        _isFiring = false;
        _fireCoroutine = null; // 코루틴 완료 표시
    }
    
    private void OnDestroy()
    {
        // 메모리 누수 방지: 오브젝트 파괴 시 모든 코루틴 정리
        if (_fireCoroutine != null)
        {
            StopCoroutine(_fireCoroutine);
            _fireCoroutine = null;
        }
    }
}