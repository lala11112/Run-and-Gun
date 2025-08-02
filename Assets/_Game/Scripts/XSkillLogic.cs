using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;

/// <summary>
/// X 스킬(스핀 공격)의 모든 데이터와 행동 로직을 포함하는 클래스입니다.
/// 이 스크립트가 붙은 프리팹의 인스펙터에서 스킬의 모든 세부 사항을 직접 설정할 수 있습니다.
/// </summary>
public class XSkillLogic : SkillBase
{
    /// <summary>
    /// X스킬의 랭크별 보너스 데이터만 담는 전용 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class XRankBonus
    {
        public StyleRank rank;
        [Tooltip("공격 범위에 곱해질 배율입니다.")]
        public float radiusMultiplier = 1f;
        [Tooltip("데미지에 곱해질 배율입니다.")]
        public float damageMultiplier = 1f;
        [Tooltip("반복 횟수에 곱해질 배율입니다.")]
        public float countMultiplier = 1f;
        [Tooltip("이 랭크에서 넉백 기능을 활성화할지 여부입니다.")]
        public bool enableKnockback = false;
        [Tooltip("이 랭크에서 투사체 반사 기능을 활성화할지 여부입니다.")]
        public bool enableProjectileReflection = false;
    }

    [Header("X-Skill 고유 데이터")]
    [Tooltip("스킬의 기본 데미지입니다.")]
    public float damage = 20f;
    [Tooltip("스킬의 기본 공격 반경입니다.")]
    public float radius = 3f;
    [Tooltip("스킬의 반복 공격 횟수입니다.")]
    public int repeatCount = 3;
    [Tooltip("반복 공격 사이의 시간 간격입니다.")]
    public float repeatDelay = 0.25f;

    [Header("이펙트 설정")]
    [Tooltip("기본 공격 범위에 맞춰 생성될 이펙트 프리팹입니다.")]
    public GameObject outerEffectPrefab;
    [Tooltip("내부 공격 범위에 맞춰 생성될 이펙트 프리팹입니다.")]
    public GameObject innerEffectPrefab;

    [Header("사운드 & 카메라")]
    [Tooltip("공격 성공 시 재생할 사운드 이름입니다.")]
    public string hitSoundName = "Swords";
    [Tooltip("공격 성공 시 사용할 카메라 쉐이크 프리셋 이름입니다.")]
    public string cameraShakePresetName = "Skill_SpinHit";

    [Header("근접 판정 상세 설정")]
    [Tooltip("전체 반경 대비 내부 판정의 비율입니다. (예: 0.5는 50%)")]
    public float innerRadiusRatio = 0.5f;
    [Tooltip("내부 판정에 맞았을 때 추가될 데미지입니다.")]
    public float innerBonusDamage = 10f;
    
    [Header("랭크별 성장 정보")]
    [Tooltip("X스킬의 랭크별 성능 변화 목록입니다.")]
    public List<XRankBonus> rankBonuses = new List<XRankBonus>();
    
    [Header("게임 설정")]
    [Tooltip("게임 전체 설정이 담긴 ScriptableObject입니다.")]
    public GameConfigSO gameConfig;
    
    // 성능 최적화: Physics 쿼리 결과 재사용을 위한 배열
    private Collider2D[] _hitResults;
    
    // 메모리 누수 방지: 진행 중인 코루틴 추적
    private Coroutine _spinCoroutine;

    private void Awake()
    {
        // 성능 최적화: 배열 크기를 GameConfig에서 가져오거나 기본값 사용
        int arraySize = gameConfig != null ? gameConfig.maxEnemyDetection : 32;
        _hitResults = new Collider2D[arraySize];
    }

    public override void Activate(GameObject caster, StyleRank currentRank)
    {
        // 이전 코루틴이 실행 중이면 중지
        if (_spinCoroutine != null)
        {
            StopCoroutine(_spinCoroutine);
        }
        _spinCoroutine = StartCoroutine(SpinRoutine(caster, currentRank));
    }

    private IEnumerator SpinRoutine(GameObject caster, StyleRank currentRank)
    {
        var rankBonus = rankBonuses.FirstOrDefault(b => b.rank == currentRank) ?? new XRankBonus();
        
        float finalRadius = radius * rankBonus.radiusMultiplier;
        float finalDamage = damage * rankBonus.damageMultiplier;
        int finalRepeats = Mathf.RoundToInt(repeatCount * rankBonus.countMultiplier);

        for (int i = 0; i < finalRepeats; i++)
        {
            PerformSpin(caster, finalRadius, finalDamage, rankBonus);
            if (i < finalRepeats - 1)
            {
                yield return new WaitForSeconds(repeatDelay);
            }
        }
    }

    private void PerformSpin(GameObject caster, float currentRadius, float currentDamage, XRankBonus rankBonus)
    {
        float innerRadius = currentRadius * innerRadiusRatio;

        CreateEffect(outerEffectPrefab, caster.transform, currentRadius);
        CreateEffect(innerEffectPrefab, caster.transform, innerRadius);

        if (!string.IsNullOrEmpty(hitSoundName)) MasterAudio.PlaySound3DAtTransform(hitSoundName, caster.transform);
        if (!string.IsNullOrEmpty(cameraShakePresetName)) CameraManager.Instance?.ShakeWithPreset(cameraShakePresetName);

        // 성능 최적화: 배열 재사용으로 GC 압박 감소
        int hitCount = Physics2D.OverlapCircleNonAlloc(caster.transform.position, currentRadius, _hitResults);
        for (int i = 0; i < hitCount; i++)
        {
            var hit = _hitResults[i];
            if (hit == null) continue;
            
            if (hit.TryGetComponent(out EnemyHealth enemyHealth))
            {
                float dealtDamage = currentDamage;
                if (Vector2.Distance(caster.transform.position, hit.transform.position) <= innerRadius)
                {
                    dealtDamage += innerBonusDamage;
                }
                enemyHealth.TakeDamage((int)dealtDamage);

                if (rankBonus.enableKnockback && hit.TryGetComponent(out Rigidbody2D enemyRb))
                {
                    Vector2 knockbackDir = (hit.transform.position - caster.transform.position).normalized;
                    float knockbackForce = gameConfig != null ? gameConfig.defaultKnockbackForce : 10f;
                    enemyRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse); 
                }
            }

            string enemyBulletTag = gameConfig != null ? gameConfig.enemyBulletTagName : "EnemyBullet";
            if (rankBonus.enableProjectileReflection && hit.CompareTag(enemyBulletTag))
            {
                Destroy(hit.gameObject);
            }
        }
        
        _spinCoroutine = null; // 코루틴 완료 표시
    }
    
    private void OnDestroy()
    {
        // 메모리 누수 방지: 오브젝트 파괴 시 모든 코루틴 정리
        if (_spinCoroutine != null)
        {
            StopCoroutine(_spinCoroutine);
            _spinCoroutine = null;
        }
    }
    
    private void CreateEffect(GameObject effectPrefab, Transform caster, float targetRadius)
    {
        if (effectPrefab == null) return;
        
        // Object Pool 사용: Instantiate 대신 풀에서 가져오기
        GameObject effectInstance = AdvancedObjectPool.Spawn(effectPrefab, caster.position, caster.rotation, caster);
        if (effectInstance == null) return; // 풀이 가득 찬 경우
        
        var ps = effectInstance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            float baseSize = ps.main.startSize.constant;
            if(baseSize > 0)
            {
                effectInstance.transform.localScale = Vector3.one * (targetRadius * 2 / baseSize);
            } 
            else 
            {
                 effectInstance.transform.localScale = Vector3.one * targetRadius * 2;
            }
            
            // 파티클 시스템 재생 시간 후 자동으로 풀로 반환
            float duration = ps.main.duration + ps.main.startLifetime.constant;
            AdvancedObjectPool.DespawnAfter(effectInstance, duration);
        }
        else
        {
             effectInstance.transform.localScale = Vector3.one * targetRadius * 2;
             // 파티클 시스템이 없으면 2초 후 반환 (기본값)
             AdvancedObjectPool.DespawnAfter(effectInstance, 2f);
        }
    }
}
