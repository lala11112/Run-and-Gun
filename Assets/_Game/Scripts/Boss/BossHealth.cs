using UnityEngine;
using System;

/// <summary>
/// 보스 체력 및 페이즈를 관리하는 컴포넌트.
/// </summary>
[RequireComponent(typeof(BossStateMachine))]
public class BossHealth : LivingEntity
{
    [Header("체력 설정")]
    [Tooltip("최대 체력")] public int maxHealth = 1000;

    [Tooltip("Phase2 (예: 70%) 시작 체력 비율")] [Range(0,1)] public float phase2Ratio = 0.7f;
    [Tooltip("Phase3 (예: 30%) 시작 체력 비율")] [Range(0,1)] public float phase3Ratio = 0.3f;

    public Action<int/*current*/,int/*max*/> OnHealthChanged;
    public Action<int/*phaseIndex*/> OnPhaseChanged; // 1,2,3
    public Action OnBossDead;

    private int _currentHealth;
    private BossStateMachine _fsm;
    private int _currentPhase = 1;

    private void Awake()
    {
        _currentHealth = maxHealth;
        _fsm = GetComponent<BossStateMachine>();
    }

    protected override void Die()
    {
        OnBossDead?.Invoke();
    }

    public override void TakeDamage(int dmg)
    {
        base.TakeDamage(dmg);   // LivingEntity 로직 실행
        CheckPhase();           // 보스 전용 로직
    }

    private void CheckPhase()
    {
        float ratio = (float)_currentHealth / maxHealth;
        if (_currentPhase == 1 && ratio <= phase2Ratio)
        {
            _currentPhase = 2;
            OnPhaseChanged?.Invoke(2);
        }
        else if (_currentPhase == 2 && ratio <= phase3Ratio)
        {
            _currentPhase = 3;
            OnPhaseChanged?.Invoke(3);
        }
    }

    public int CurrentPhase => _currentPhase;
    public int CurrentHealth => _currentHealth;
} 