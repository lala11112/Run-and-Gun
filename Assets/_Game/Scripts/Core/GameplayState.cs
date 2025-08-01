using System.Collections;
using UnityEngine;

/// <summary>
/// GameplayState – 실제 게임플레이 씬을 로드하고 게임 모드 실행을 시작합니다.
/// </summary>
public class GameplayState : IState
{
    private readonly StateMachine _sm;
    private readonly GameManager _gm;
    
    public GameplayState(StateMachine sm, GameManager gm)
    {
        _sm = sm;
        _gm = gm;
    }

    public void Enter()
    {
        Debug.Log("[GameplayState] Enter");
        // 게임플레이 상태에 진입했을 때 필요한 초기화 로직 (예: HUD 표시)
    }

    public void Exit()
    {
        Debug.Log("[GameplayState] Exit");
        // 게임플레이 상태를 떠날 때 필요한 정리 로직 (예: HUD 숨기기)
    }

    public void Tick()
    {
        // 게임플레이 중 매 프레임 실행되어야 할 로직
    }
} 