using System;

/// <summary>
/// 간단하면서도 확장 가능한 상태 머신 클래스입니다.
/// 현재 활성 상태를 보관하고, 상태 전환(ChangeState) 및 매 프레임 Tick 호출을 담당합니다.
/// </summary>
public class StateMachine
{
    /// <summary>현재 활성화된 상태(읽기 전용)</summary>
    public IState CurrentState { get; private set; }

    /// <summary>
    /// 상태를 변경합니다.
    /// </summary>
    public void ChangeState(IState nextState)
    {
        if (nextState == null) throw new ArgumentNullException(nameof(nextState));
        if (nextState == CurrentState) return;

        // 기존 상태 Exit
        CurrentState?.Exit();

        // 새 상태 Enter
        CurrentState = nextState;
        CurrentState.Enter();

        // 상태 변경 브로드캐스트
        GameEvents.RaiseStateChanged(CurrentState);
    }

    /// <summary>
    /// 매 프레임 호출하여 현재 상태의 Tick을 실행합니다.
    /// </summary>
    public void Tick()
    {
        CurrentState?.Tick();
    }
} 