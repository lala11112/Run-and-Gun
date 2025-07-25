using UnityEngine;
using System.Collections;

/// <summary>
/// 보스 "Xenon Commander"의 상위 컨트롤러.
/// FSM, 페이즈, 패턴 매니저를 연결하고 상태 객체를 생성한다.
/// </summary>
[RequireComponent(typeof(BossStateMachine))]
[RequireComponent(typeof(BossHealth))]
[RequireComponent(typeof(BossPatternManager))]
public class XenonCommanderController : MonoBehaviour
{
    private BossStateMachine _fsm;
    private BossHealth _health;
    private BossPatternManager _patterns;

    private void Awake()
    {
        _fsm = GetComponent<BossStateMachine>();
        _health = GetComponent<BossHealth>();
        _patterns = GetComponent<BossPatternManager>();

        // 보스 사망 이벤트 → DeadState 전환
        _health.OnBossDead += () => _fsm.SetState(new DeadState());
        _health.OnPhaseChanged += phase => _fsm.SetState(new PhaseState(phase));

        // 인트로 상태부터 시작
        _fsm.SetState(new IntroState());
    }

    // ---------------- 상태 구현 ----------------

    /// <summary>보스 등장 연출</summary>
    private class IntroState : BossState
    {
        public override void Enter()
        {
            // TODO: 등장 애니메이션 / 카메라 줌 연출
        }
        public override void Tick(){}
        public override void Exit(){}
    }

    /// <summary>페이즈별 전투 상태</summary>
    private class PhaseState : BossState
    {
        private readonly int _phase;
        public PhaseState(int p)=>_phase=p;
        public override void Enter()
        {
            // 패턴 매니저가 자동으로 페이즈별 리스트 실행
        }
        public override void Tick(){}
        public override void Exit(){}
    }

    /// <summary>사망 상태</summary>
    private class DeadState : BossState
    {
        public override void Enter()
        {
            // 연출은 BossPresentation에서 이미 처리
        }
        public override void Tick(){}
        public override void Exit(){}
    }
} 