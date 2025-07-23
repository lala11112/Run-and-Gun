using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 기존 Enemy 기능과 호환되는 래퍼 – Enemy 프리팹을 단계적으로 이 클래스로 교체할 수 있다.
/// </summary>
[RequireComponent(typeof(NavMeshMovement))]
public class SimpleEnemy : EnemyCore
{
    // EnemyCore 이미 statData 필드를 보유하므로 중복 선언을 제거했습니다.

    // 모듈 참조
    private IMovement  _movement;
    private IStunnable _stun;
    private EnemyHealth _health;

    // ---- 외부에 노출할 공용 게이트웨이 ----
    public float MoveSpeed    => _movement?.MoveSpeed    ?? 0f;
    public float KeepDistance => _movement?.KeepDistance ?? 0f;
    public bool  IsStunned    => _stun != null && _stun.IsStunned;
    public void  Stun(float dur) => _stun?.Stun(dur);

    public event System.Action<int,int> OnHealthChanged
    {
        add { if (_health != null) _health.OnHealthChanged += value; }
        remove { if (_health != null) _health.OnHealthChanged -= value; }
    }

    protected override void Awake()
    {
        base.Awake();

        _movement = GetComponent<IMovement>();
        _stun     = GetComponent<IStunnable>(); // 선택적
        _health   = GetComponent<EnemyHealth>();

        // SO 스탯 적용
        if (statData != null && _health != null)
        {
            _health.maxHealth    = statData.maxHealth;
            _health.currentHealth = statData.maxHealth;
            if (_movement is NavMeshMovement nm)
            {
                nm.moveSpeed    = statData.moveSpeed;
                nm.keepDistance = statData.keepDistance;
            }
        }
    }

    protected override void Die()
    {
        Destroy(gameObject);
    }

    // ---------- 데미지 전달 ----------
    public override void TakeDamage(int dmg)
    {
        // EnemyHealth가 실제 체력 관리 담당
        if (_health != null)
        {
            _health.TakeDamage(dmg);
        }
        else
        {
            base.TakeDamage(dmg); // fallback
        }
    }
} 