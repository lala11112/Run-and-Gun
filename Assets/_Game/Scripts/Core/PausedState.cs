using UnityEngine;

/// <summary>
/// PausedState – Time.timeScale 을 0으로 설정하여 게임을 일시정지합니다.
/// </summary>
public class PausedState : IState
{
    private readonly StateMachine _sm;
    private readonly GameManager _gm;
    private readonly IState _resumeState;
    private readonly GameObject _menuPrefab;
    private readonly PauseMenuContext _context;
    private float _prevTimeScale;
    private GameObject _menuInstance;

    public PausedState(StateMachine sm, GameManager gm, IState resumeState, GameObject menuPrefab, PauseMenuContext context)
    {
        _sm = sm;
        _gm = gm;
        _resumeState = resumeState;
        _menuPrefab = menuPrefab;
        _context = context;
    }

    public void Enter()
    {
        Debug.Log("[PausedState] Enter – Pause 활성");
        _prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        // DOTween, AudioMixer 등 일시정지 처리 필요시 추가

        // Pause 메뉴 표시
        if (_menuPrefab != null)
        {
            _menuInstance = Object.Instantiate(_menuPrefab);
            var controller = _menuInstance.GetComponent<PauseMenuControllerBase>();
            controller?.Initialize(_context);
            UIManager.Instance?.Push(_menuInstance);
        }
    }

    public void Exit()
    {
        Debug.Log("[PausedState] Exit – Pause 해제");
        Time.timeScale = _prevTimeScale;

        // UIManager 스택에서 UI를 제거하고, 생성했던 인스턴스를 파괴합니다.
        UIManager.Instance?.Pop();
    }

    public void Tick() { }

    // Pause 해제 메서드(ESC 등 입력에서 호출)
    public void Resume()
    {
        // 이제 Resume은 상태 변경만 트리거합니다. 뒷정리는 Exit()이 알아서 합니다.
        _sm.ChangeState(_resumeState);
    }
} 