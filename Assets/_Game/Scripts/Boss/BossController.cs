using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 개선된 보스 컨트롤러입니다.
/// BossDataSO를 기반으로 유연하고 확장 가능한 보스 시스템을 제공합니다.
/// </summary>
[RequireComponent(typeof(BossStateMachine))]
[RequireComponent(typeof(BossHealth))]
public class BossController : MonoBehaviour
{
    [Header("보스 설정")]
    [Tooltip("이 보스의 데이터 ScriptableObject")]
    public BossDataSO bossData;

    [Header("디버그")]
    [Tooltip("현재 페이즈 (읽기 전용)")]
    [SerializeField] private int _currentPhase = 0;
    
    [Tooltip("현재 실행 중인 패턴 (읽기 전용)")]
    [SerializeField] private string _currentPatternName = "None";

    // 컴포넌트 참조
    private BossStateMachine _stateMachine;
    private BossHealth _health;
    private BossPresentation _presentation;
    
    // 상태 관리
    private Coroutine _patternCoroutine;
    private bool _isDead = false;
    
    // 이벤트
    public event Action<int> OnPhaseChanged;
    public event Action<BossPatternSO> OnPatternStarted;
    public event Action<BossPatternSO> OnPatternCompleted;

    #region Properties
    
    public BossDataSO BossData => bossData;
    public int CurrentPhase => _currentPhase;
    public bool IsDead => _isDead;
    public Transform Transform => transform;
    public GameObject GameObject => gameObject;
    
    #endregion

    private void Awake()
    {
        InitializeComponents();
        ValidateBossData();
        SetupEventHandlers();
    }

    private void Start()
    {
        InitializeBoss();
    }

    private void InitializeComponents()
    {
        _stateMachine = GetComponent<BossStateMachine>();
        _health = GetComponent<BossHealth>();
        _presentation = GetComponent<BossPresentation>();
        
        if (_stateMachine == null)
        {
            Debug.LogError($"[BossController] {gameObject.name}에 BossStateMachine이 없습니다.");
        }
        
        if (_health == null)
        {
            Debug.LogError($"[BossController] {gameObject.name}에 BossHealth가 없습니다.");
        }
    }

    private void ValidateBossData()
    {
        if (bossData == null)
        {
            Debug.LogError($"[BossController] {gameObject.name}의 BossData가 설정되지 않았습니다.");
            return;
        }
        
        if (bossData.phases.Count == 0)
        {
            Debug.LogWarning($"[BossController] {bossData.bossName}에 페이즈가 설정되지 않았습니다.");
        }
    }

    private void SetupEventHandlers()
    {
        if (_health != null)
        {
            _health.OnHealthChanged += HandleHealthChanged;
            _health.OnBossDead += HandleBossDeath;
        }
    }

    private void InitializeBoss()
    {
        if (bossData == null) return;
        
        // 보스 체력 설정
        if (_health != null)
        {
            _health.bossMaxHealth = bossData.maxHealth;
            _health.defense = bossData.defense;
        }
        
        // 첫 번째 페이즈로 시작
        StartPhase(0);
        
        // 인트로 상태로 시작
        _stateMachine?.SetState(new BossIntroState(this));
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        if (bossData == null) return;
        
        float healthRatio = (float)currentHealth / maxHealth;
        int newPhase = bossData.GetPhaseForHealthRatio(healthRatio);
        
        if (newPhase != _currentPhase)
        {
            StartPhase(newPhase);
        }
    }

    private void HandleBossDeath()
    {
        _isDead = true;
        StopAllPatterns();
        _stateMachine?.SetState(new BossDeathState(this));
    }

    /// <summary>
    /// 지정된 페이즈를 시작합니다.
    /// </summary>
    public void StartPhase(int phaseIndex)
    {
        if (bossData == null || phaseIndex < 0 || phaseIndex >= bossData.phases.Count)
        {
            return;
        }
        
        _currentPhase = phaseIndex;
        var phaseData = bossData.GetPhaseData(phaseIndex);
        
        Debug.Log($"[BossController] {bossData.bossName} - {phaseData.phaseName} 시작");
        
        // 이전 패턴 중지
        StopAllPatterns();
        
        // 새 페이즈 시작
        _stateMachine?.SetState(new BossCombatState(this, phaseData));
        
        // 이벤트 발생
        OnPhaseChanged?.Invoke(phaseIndex);
    }

    /// <summary>
    /// 패턴을 실행합니다.
    /// </summary>
    public void ExecutePattern(BossPatternSO pattern)
    {
        if (pattern == null || _isDead) return;
        
        if (_patternCoroutine != null)
        {
            StopCoroutine(_patternCoroutine);
        }
        
        _patternCoroutine = StartCoroutine(ExecutePatternCoroutine(pattern));
    }

    private IEnumerator ExecutePatternCoroutine(BossPatternSO pattern)
    {
        _currentPatternName = pattern.patternName;
        OnPatternStarted?.Invoke(pattern);
        
        yield return StartCoroutine(pattern.ExecutePattern(this));
        
        _currentPatternName = "None";
        OnPatternCompleted?.Invoke(pattern);
        _patternCoroutine = null;
    }

    /// <summary>
    /// 모든 패턴을 중지합니다.
    /// </summary>
    public void StopAllPatterns()
    {
        if (_patternCoroutine != null)
        {
            StopCoroutine(_patternCoroutine);
            _patternCoroutine = null;
        }
        _currentPatternName = "None";
    }

    /// <summary>
    /// 플레이어를 찾아 반환합니다.
    /// </summary>
    public Transform FindPlayer()
    {
        var playerObj = GameObject.FindWithTag("Player");
        return playerObj?.transform;
    }

    /// <summary>
    /// 보스와 플레이어 사이의 거리를 반환합니다.
    /// </summary>
    public float GetDistanceToPlayer()
    {
        var player = FindPlayer();
        if (player == null) return float.MaxValue;
        
        return Vector3.Distance(transform.position, player.position);
    }

    /// <summary>
    /// 플레이어 방향의 벡터를 반환합니다.
    /// </summary>
    public Vector3 GetDirectionToPlayer()
    {
        var player = FindPlayer();
        if (player == null) return Vector3.zero;
        
        return (player.position - transform.position).normalized;
    }

    private void OnDestroy()
    {
        StopAllPatterns();
        
        if (_health != null)
        {
            _health.OnHealthChanged -= HandleHealthChanged;
            _health.OnBossDead -= HandleBossDeath;
        }
    }

    #region 에디터 지원
    
    [ContextMenu("페이즈 정보 출력")]
    private void LogPhaseInfo()
    {
        if (bossData == null)
        {
            Debug.Log("BossData가 설정되지 않았습니다.");
            return;
        }
        
        Debug.Log($"=== {bossData.bossName} 페이즈 정보 ===");
        for (int i = 0; i < bossData.phases.Count; i++)
        {
            var phase = bossData.phases[i];
            Debug.Log($"페이즈 {i}: {phase.phaseName} (체력 {phase.healthRatio * 100}% 이상)");
            Debug.Log($"  패턴 수: {phase.patterns.Count}개");
        }
    }
    
    #endregion
}