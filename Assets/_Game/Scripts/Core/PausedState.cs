using UnityEngine;

/// <summary>
/// PausedState – Time.timeScale 을 0으로 설정하여 게임을 일시정지합니다.
/// </summary>
public class PausedState : IState
{
    private readonly StateMachine _sm;
    private readonly GameManager _gm;
    private readonly IState _resumeState;
    private float _prevTimeScale;

    public PausedState(StateMachine sm, GameManager gm, IState resumeState)
    {
        _sm = sm;
        _gm = gm;
        _resumeState = resumeState;
    }

    public void Enter()
    {
        Debug.Log("[PausedState] Enter – Pause 활성");
        _prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        // DOTween, AudioMixer 등 일시정지 처리 필요시 추가

        // Pause 메뉴 표시
        var prefab = Resources.Load<GameObject>("UI/PauseMenu");
        if (prefab != null)
        {
            var panel = Object.Instantiate(prefab);
            UIManager.Instance?.Push(panel);
        }
    }

    public void Exit()
    {
        Debug.Log("[PausedState] Exit – Pause 해제");
        Time.timeScale = _prevTimeScale;

        // 메뉴 숨김
        UIManager.Instance?.Pop();
    }

    public void Tick() { }

    // Pause 해제 메서드(ESC 등 입력에서 호출)
    public void Resume()
    {
        _sm.ChangeState(_resumeState);
    }
} 