using System.Collections;
using UnityEngine;

/// <summary>
/// TitleState – 타이틀 씬에서 메뉴 입력을 대기합니다.
/// 데모 목적으로, 몇 초 후 자동으로 GameplayState 로 전환합니다.
/// 실제 게임에서는 UI 버튼의 콜백에서 GameStart() 를 호출하도록 변경하세요.
/// </summary>
public class TitleState : IState
{
    private readonly StateMachine _sm;
    private readonly GameManager _gm;

    public TitleState(StateMachine sm, GameManager gm)
    {
        _sm = sm;
        _gm = gm;
    }

    public void Enter()
    {
        Debug.Log("[TitleState] Enter – 타이틀 UI 표시");
        // 필요하다면 BGM 재생, UI 활성화 등을 여기서 처리
        _gm.StartCoroutine(DemoAutoStart());
    }

    public void Exit()
    {
        Debug.Log("[TitleState] Exit – 타이틀 UI 정리");
    }

    public void Tick() { }

    // --------- 데모용 자동 시작 ---------
    private IEnumerator DemoAutoStart()
    {
        yield return new WaitForSeconds(1.0f);
        StartGame();
    }

    // 실제 게임 시작 호출
    public void StartGame()
    {
        _sm.ChangeState(new GameplayState(_sm, _gm));
    }
} 