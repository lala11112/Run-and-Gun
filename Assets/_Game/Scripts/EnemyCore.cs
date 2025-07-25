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