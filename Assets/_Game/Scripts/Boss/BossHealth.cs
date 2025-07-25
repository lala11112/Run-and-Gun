using UnityEngine;
using System;
using UnityEngine.Serialization;

/// <summary>
/// 보스 체력 및 페이즈를 관리하는 컴포넌트.
/// </summary>
[RequireComponent(typeof(BossStateMachine))]
public class BossHealth : LivingEntity
{
    /// <summary>보스가 씬에 생성될 때 발생하는 전역 이벤트</summary>
    public static event Action<BossHealth> OnBossSpawned;

    [Header("기본 체력 설정")]
    [Tooltip("보스 최대 체력")] [FormerlySerializedAs("maxHealth")] public int bossMaxHealth = 1000;

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
        // EnemyStatData가 지정되어 있으면 해당 값으로 덮어쓰기
        if (TryGetComponent(out EnemyCore core) && core.statData != null)
        {
            bossMaxHealth = core.statData.maxHealth;
        }

        // LivingEntity 초기화 수행 (currentHealth 설정)
        maxHealth = bossMaxHealth;
        base.Awake();

        _fsm = GetComponent<BossStateMachine>();

        // 전역 이벤트 발송 – UI 등에서 보스 등장 인식
        OnBossSpawned?.Invoke(this);
    }

    protected override void Die()
    {
        OnBossDead?.Invoke();
        GameEvents.RaiseEnemyDied(true);
        Destroy(gameObject);
    }

    public override void TakeDamage(int dmg)
    {
        base.TakeDamage(dmg); // LivingEntity 감소 및 Die 호출
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        CheckPhase();
    }

    private void CheckPhase()
    {
        float ratio = (float)currentHealth / maxHealth;
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
} 