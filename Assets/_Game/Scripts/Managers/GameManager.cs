using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.InputSystem;

/// <summary>
/// 게임 전역 상태를 관리하는 싱글톤 매니저.
/// Boot, Title, InGame, Paused, Result 등의 큰 흐름만 담당합니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("씬 이름 설정")]
    [Tooltip("타이틀 씬 이름")] public string titleSceneName = "Title";
    [Tooltip("결과 화면 씬 이름")] public string resultSceneName = "Result";

    [Tooltip("현재 게임 상태(읽기 전용)")] public GameState CurrentState { get; private set; } = GameState.Boot;
    
    // ---------------- 일시정지 ----------------
    public bool IsPaused { get; private set; }
    public event Action<bool> OnPauseToggled;
    
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
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        if (_stateMachine == null) return;
        
        // TODO: 일시정지 메뉴 로직을 새로운 시스템에 맞게 재구성해야 합니다.
        // 현재는 PausedState로 직접 전환하는 기능이 없습니다.
        // if (_stateMachine.CurrentState is GameplayState gameplayState)
        // {
        //     gameplayState.PauseGame();
        // }
        // else if (_stateMachine.CurrentState is PausedState paused)
        // {
        //     paused.Resume();
        // }
    }
    
    /// <summary>
    /// 게임 상태를 변경하고, 필요 시 씬을 로드합니다.
    /// </summary>
    public void ChangeState(GameState newState, bool loadScene = false)
    {
        if (newState == CurrentState && !loadScene) return;

        CurrentState = newState;
        
        IState nextState = newState switch
        {
            GameState.Title => new TitleState(_stateMachine, this),
            GameState.InGame => new GameplayState(_stateMachine, this),
            // GameState.Paused => new PausedState(...), // 일시정지는 별도 처리가 필요합니다.
            GameState.GameOver => new ResultState(_stateMachine, this),
            _ => new BootState(_stateMachine, this)
        };
        
        _stateMachine.ChangeState(nextState);
    }
    
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
        // 플레이어가 사망하면 게임 오버 상태로 전환합니다.
        ChangeState(GameState.GameOver, true);
    }

    /// <summary>
    /// UI 버튼 등에서 호출 – 타이틀 화면으로 복귀
    /// </summary>
    public void ReturnToTitle()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        ChangeState(GameState.Title, true);
    }

    // 외부에서 상태 머신 참조가 필요할 때 사용 (사용 최소화 권장)
    public StateMachine GetStateMachine() => _stateMachine;
}
