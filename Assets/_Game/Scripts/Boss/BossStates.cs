using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 보스 인트로 상태 - 등장 연출을 담당합니다.
/// </summary>
public class BossIntroState : BossState
{
    private readonly BossController _controller;
    private bool _introCompleted = false;

    public BossIntroState(BossController controller)
    {
        _controller = controller;
    }

    public override void Enter()
    {
        Debug.Log($"[BossIntroState] {_controller.BossData.bossName} 등장!");
        
        // TODO: 등장 애니메이션, 카메라 연출 등
        _controller.StartCoroutine(PlayIntroSequence());
    }

    public override void Tick()
    {
        // 인트로가 완료되면 전투 상태로 전환
        if (_introCompleted)
        {
            var firstPhase = _controller.BossData.GetPhaseData(0);
            _controller.GetComponent<BossStateMachine>()?.SetState(new BossCombatState(_controller, firstPhase));
        }
    }

    public override void Exit()
    {
        Debug.Log($"[BossIntroState] {_controller.BossData.bossName} 전투 시작!");
    }

    private IEnumerator PlayIntroSequence()
    {
        // 간단한 인트로 시퀀스 (실제로는 더 복잡한 연출 가능)
        yield return new WaitForSeconds(2f);
        _introCompleted = true;
    }
}

/// <summary>
/// 보스 전투 상태 - 페이즈별 패턴을 실행합니다.
/// </summary>
public class BossCombatState : BossState
{
    private readonly BossController _controller;
    private readonly BossDataSO.PhaseData _phaseData;
    private Coroutine _patternLoopCoroutine;
    private int _currentPatternIndex = 0;

    public BossCombatState(BossController controller, BossDataSO.PhaseData phaseData)
    {
        _controller = controller;
        _phaseData = phaseData;
    }

    public override void Enter()
    {
        Debug.Log($"[BossCombatState] {_phaseData.phaseName} 진입");
        
        // 페이즈 전환 메시지 출력
        if (!string.IsNullOrEmpty(_phaseData.transitionMessage))
        {
            Debug.Log($"[Boss] {_phaseData.transitionMessage}");
        }
        
        // 패턴 루프 시작
        if (_phaseData.patterns.Count > 0)
        {
            _patternLoopCoroutine = _controller.StartCoroutine(PatternLoop());
        }
    }

    public override void Tick()
    {
        // 전투 중 특별한 로직이 필요하면 여기에 추가
    }

    public override void Exit()
    {
        // 패턴 루프 중지
        if (_patternLoopCoroutine != null)
        {
            _controller.StopCoroutine(_patternLoopCoroutine);
            _patternLoopCoroutine = null;
        }
        
        _controller.StopAllPatterns();
    }

    private IEnumerator PatternLoop()
    {
        while (!_controller.IsDead)
        {
            if (_phaseData.patterns.Count == 0)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            // 현재 패턴 실행
            var pattern = _phaseData.patterns[_currentPatternIndex];
            if (pattern != null)
            {
                yield return _controller.StartCoroutine(pattern.ExecutePattern(_controller));
            }

            // 다음 패턴으로 이동
            _currentPatternIndex = (_currentPatternIndex + 1) % _phaseData.patterns.Count;
            
            // 잠깐 대기
            yield return new WaitForSeconds(0.5f);
        }
    }
}

/// <summary>
/// 보스 사망 상태 - 사망 연출을 담당합니다.
/// </summary>
public class BossDeathState : BossState
{
    private readonly BossController _controller;
    private bool _deathSequenceStarted = false;

    public BossDeathState(BossController controller)
    {
        _controller = controller;
    }

    public override void Enter()
    {
        Debug.Log($"[BossDeathState] {_controller.BossData.bossName} 사망");
        
        if (!_deathSequenceStarted)
        {
            _deathSequenceStarted = true;
            _controller.StartCoroutine(PlayDeathSequence());
        }
    }

    public override void Tick()
    {
        // 사망 상태에서는 특별한 로직 없음
    }

    public override void Exit()
    {
        // 사망 상태에서는 다른 상태로 전환되지 않음
    }

    private IEnumerator PlayDeathSequence()
    {
        // 사망 연출 (BossPresentation에서 처리되지만 추가 로직 가능)
        yield return new WaitForSeconds(1f);
        
        // 보상 지급
        GiveRewards();
        
        Debug.Log($"[BossDeathState] {_controller.BossData.bossName} 처치 완료!");
    }

    private void GiveRewards()
    {
        var bossData = _controller.BossData;
        
        // 골드 보상
        int goldReward = Mathf.RoundToInt(bossData.maxHealth * bossData.goldRewardMultiplier);
        CurrencyService.Instance?.AddGold(goldReward);
        
        // 경험치 보상 (필요시)
        // ExperienceService.Instance?.AddExperience(bossData.experienceReward);
        
        Debug.Log($"[Reward] 골드 {goldReward} 획득!");
    }
}

/// <summary>
/// 보스 스턴 상태 - 일시적으로 무력화된 상태입니다.
/// </summary>
public class BossStunState : BossState
{
    private readonly BossController _controller;
    private readonly float _stunDuration;
    private float _stunTimer;

    public BossStunState(BossController controller, float duration = 3f)
    {
        _controller = controller;
        _stunDuration = duration;
        _stunTimer = duration;
    }

    public override void Enter()
    {
        Debug.Log($"[BossStunState] {_controller.BossData.bossName} 스턴됨 ({_stunDuration}초)");
        
        // 모든 패턴 중지
        _controller.StopAllPatterns();
        
        // 스턴 이펙트 재생 (필요시)
        // EffectManager.Instance?.PlayEffect("StunEffect", _controller.transform.position);
    }

    public override void Tick()
    {
        _stunTimer -= Time.deltaTime;
        
        // 스턴 시간이 끝나면 전투 상태로 복귀
        if (_stunTimer <= 0f)
        {
            var currentPhase = _controller.BossData.GetPhaseData(_controller.CurrentPhase);
            _controller.GetComponent<BossStateMachine>()?.SetState(new BossCombatState(_controller, currentPhase));
        }
    }

    public override void Exit()
    {
        Debug.Log($"[BossStunState] {_controller.BossData.bossName} 스턴 해제");
    }
}