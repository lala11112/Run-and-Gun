using UnityEngine;

/// <summary>
/// 모든 적 캐릭터의 최소 공통 베이스.
/// 이동·공격 모듈을 조합해 동작하며, 체력/사망 처리 로직을 제공한다.
/// </summary>
public abstract class EnemyCore : MonoBehaviour, IDamageable
{
    [Header("스탯 데이터 (선택)")]
    public EnemyStatData statData;
    [Tooltip("최대 체력 (SO 없는 경우 사용)")] public int baseMaxHp = 5;

    protected int currentHp;

    protected IMovement movement;
    protected IAttack attack;
    protected IHitEffect hitFx;

    protected virtual void Awake()
    {
        movement = GetComponent<IMovement>();
        attack   = GetComponent<IAttack>();
        hitFx    = GetComponent<IHitEffect>();

        currentHp = statData != null ? statData.maxHealth : baseMaxHp;
        
        // NavMeshAgent 유효성 검사 및 위치 보정
        if (TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
        {
            if (!agent.isOnNavMesh)
            {
                // NavMesh 위가 아니라면, 가장 가까운 NavMesh의 점을 찾아 위치를 보정합니다.
                if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out var hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                    Debug.LogWarning($"'{name}'이(가) NavMesh 밖에 스폰되어 가까운 지점으로 이동시켰습니다.", gameObject);
                }
                else
                {
                    Debug.LogError($"'{name}' 주변 10m 내에 NavMesh를 찾을 수 없습니다. 스폰 위치나 NavMesh 베이크를 확인하세요.", gameObject);
                    // 에이전트를 비활성화하여 추가 오류를 방지합니다.
                    agent.enabled = false; 
                }
            }
        }
    }

    protected virtual void Update()
    {
        OnTick();
    }

    protected virtual void OnTick()
    {
        // 기본 구현: 플레이어 추적 이동
        if (movement != null)
        {
            var player = GameObject.FindWithTag("Player")?.transform;
            if (player != null) movement.Move(player);
        }
    }

    public virtual void TakeDamage(int dmg)
    {
        currentHp -= dmg;
        if (currentHp < 0) currentHp = 0;
        hitFx?.PlayHit(dmg);
        if (currentHp <= 0) Die();
    }

    protected abstract void Die();
} 