using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.InputSystem;
/// <summary>
/// 게임 전역 상태를 관리하는 싱글톤 매니저.
/// 전환 빈도가 낮은 Boot/Title/InGame/Pause/GameOver 흐름만 담당합니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("씬 이름 설정")]
    [Tooltip("타이틀 씬 이름")] public string titleSceneName = "Title";
    [Tooltip("게임플레이 씬 이름")] public string gameplaySceneName = "Gameplay";
    [Tooltip("결과 화면 씬 이름")] public string resultSceneName = "Result";

    [Tooltip("현재 게임 상태(읽기 전용)")] public GameState CurrentState { get; private set; } = GameState.Boot;

    /// <summary>게임 상태 변경 이벤트: (새 상태)</summary>
    public event Action<GameState> OnStateChanged;

    /// <summary>현재 선택된 게임 모드 전략 객체.</summary>
    private IGameMode _currentMode;

    private readonly Dictionary<GameModeType, IGameMode> _modes = new();

    // ---------------- 일시정지 ----------------
    public bool IsPaused { get; private set; }
    public event Action<bool> OnPauseToggled;

    /// <summary>직전 런(플레이) 결과 승리 여부</summary>
    public bool LastRunVictory { get; set; }

    [Header("Input")]
    [Tooltip("일시정지에 사용할 InputActionReference (Button)")] public InputActionReference pauseAction;

    private StateMachine _stateMachine;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (pauseAction != null)
        {
            pauseAction.action.performed += OnPausePerformed;
            pauseAction.action.Enable();
        }

        RegisterModes();

        _stateMachine = new StateMachine();
        _stateMachine.ChangeState(new BootState(_stateMachine, this));

        GameEvents.StateChanged += OnGlobalStateChanged;
        GameEvents.PlayerDied += OnPlayerDied;
    }

    private void OnDestroy()
    {
        if (pauseAction != null)
        {
            pauseAction.action.performed -= OnPausePerformed;
        }

        GameEvents.StateChanged -= OnGlobalStateChanged;
        GameEvents.PlayerDied -= OnPlayerDied;
    }

    private void Update()
    {
        _stateMachine?.Tick();

        // 구 InputManager 대비 – 새 Input System은 이벤트로 처리, 이 Update 백업키는 삭제 가능합니다.
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        if (_stateMachine == null) return;

        if (_stateMachine.CurrentState is GameplayState gameplay)
        {
            _stateMachine.ChangeState(new PausedState(_stateMachine, this, gameplay));
        }
        else if (_stateMachine.CurrentState is PausedState paused)
        {
            paused.Resume();
        }
    }

    /// <summary>
    /// 게임 상태를 변경합니다.
    /// </summary>
    public void ChangeState(GameState newState)
    {
        if (newState == CurrentState) return;
        // 씬 전환 트리거 (간단 매핑)
        if (newState == GameState.Title)
        {
            StartCoroutine(SceneLoader.LoadSceneAsync(titleSceneName));
        }
        else if (newState == GameState.InGame)
        {
            StartCoroutine(SceneLoader.LoadSceneAsync(gameplaySceneName));
        }

        CurrentState = newState;
        OnStateChanged?.Invoke(CurrentState);
    }

    // ---------------- Pause Logic ----------------
    public void TogglePause()
    {
        SetPause(!IsPaused);
    }

    public void SetPause(bool pause)
    {
        if (pause == IsPaused) return;
        IsPaused = pause;

        Time.timeScale = pause ? 0f : 1f;
        DOTween.TogglePauseAll(); // DOTween 전체 일시정지/재개

        OnPauseToggled?.Invoke(IsPaused);
        ChangeState(pause ? GameState.Paused : GameState.InGame);
    }

    /// <summary>
    /// 게임 모드를 전환하고 InGame 상태로 진입합니다.
    /// </summary>
    public void SwitchMode(GameModeType type)
    {
        if (_currentMode != null)
        {
            _currentMode.OnRunEnded -= HandleRunEnded;
            _currentMode.Cleanup();
        }
        if (_modes.TryGetValue(type, out var mode))
        {
            _currentMode = mode;
            _currentMode.OnRunEnded += HandleRunEnded;
            _currentMode.Initialize();
            ChangeState(GameState.InGame);
        }
        else
        {
            Debug.LogError($"[GameManager] 등록되지 않은 모드: {type}");
        }
    }

    private void RegisterModes()
    {
        _modes[GameModeType.Roguelike] = new RoguelikeMode();
        _modes[GameModeType.Story]    = new StoryMode();
    }

    private void HandleRunEnded(bool victory)
    {
        // TODO: victory 값에 따라 결과 처리 (성공/실패 분기)
        StartCoroutine(SceneLoader.LoadSceneAsync(resultSceneName));
        ChangeState(GameState.GameOver);
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>디버그용 강제 게임오버(또는 승리) 트리거</summary>
    public void DebugForceGameOver(bool victory = false)
    {
        StartCoroutine(SceneLoader.LoadSceneAsync(resultSceneName));
        ChangeState(GameState.GameOver);
    }
#endif

    private void OnGlobalStateChanged(IState st)
    {
        // GameState 프로퍼티 갱신 (HUD, UI 등에서 사용)
        if      (st is BootState)      CurrentState = GameState.Boot;
        else if (st is TitleState)     CurrentState = GameState.Title;
        else if (st is GameplayState)  CurrentState = GameState.InGame;
        else if (st is PausedState)    CurrentState = GameState.Paused;
        else if (st is ResultState)    CurrentState = GameState.GameOver;
    }

    private void OnPlayerDied()
    {
        LastRunVictory = false;
        _stateMachine?.ChangeState(new ResultState(_stateMachine, this, false));
    }

    /// <summary>
    /// UI 버튼 등에서 호출 – 타이틀 화면으로 복귀
    /// </summary>
    public void ReturnToTitle()
    {
        // Pause 해제
        Time.timeScale = 1f;
        IsPaused = false;
        // 상태 머신을 타이틀로 전환
        _stateMachine?.ChangeState(new TitleState(_stateMachine, this));
    }

    /// <summary>
    /// Pause 메뉴의 Resume 버튼에서 호출 – PausedState 를 종료하고 Gameplay 로 복귀합니다.
    /// </summary>
    public void ResumeFromPause()
    {
        if (_stateMachine?.CurrentState is PausedState paused)
        {
            paused.Resume();
        }
    }
} 