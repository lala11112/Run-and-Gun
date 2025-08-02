using UnityEngine;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 스토리 모드 디버깅을 위한 런타임 도구입니다.
/// </summary>
public class StoryDebugger : MonoBehaviour
{
    [Header("디버그 UI 설정")]
    [Tooltip("디버그 UI를 표시할지 여부")]
    public bool showDebugUI = true;
    
    [Tooltip("디버그 키 (기본: F1)")]
    public KeyCode debugKey = KeyCode.F1;
    
    [Header("로그 설정")]
    [Tooltip("스텝 실행 로그를 파일로 저장할지 여부")]
    public bool saveLogToFile = false;
    
    private bool _showDebugWindow = false;
    private Vector2 _scrollPosition = Vector2.zero;
    private List<string> _executionLog = new List<string>();
    private const int MAX_LOG_ENTRIES = 100;
    
    private void Awake()
    {
        // 스토리 플레이어 이벤트 구독
        if (StoryPlayer.Instance != null)
        {
            StoryPlayer.Instance.OnStepChanged += OnStepChanged;
            StoryPlayer.Instance.OnPauseStateChanged += OnPauseStateChanged;
            StoryPlayer.Instance.OnChapterComplete += OnChapterComplete;
        }
        
        // 컨텍스트 이벤트 구독
        StoryPlayerContext.OnVariableChanged += OnVariableChanged;
        StoryPlayerContext.OnObjectChanged += OnObjectChanged;
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (StoryPlayer.Instance != null)
        {
            StoryPlayer.Instance.OnStepChanged -= OnStepChanged;
            StoryPlayer.Instance.OnPauseStateChanged -= OnPauseStateChanged;
            StoryPlayer.Instance.OnChapterComplete -= OnChapterComplete;
        }
        
        StoryPlayerContext.OnVariableChanged -= OnVariableChanged;
        StoryPlayerContext.OnObjectChanged -= OnObjectChanged;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(debugKey))
        {
            _showDebugWindow = !_showDebugWindow;
        }
    }
    
    private void OnGUI()
    {
        if (!showDebugUI || !_showDebugWindow) return;
        
        // 디버그 윈도우 그리기
        GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("스토리 디버거", GUI.skin.GetStyle("label"));
        GUILayout.Space(10);
        
        DrawStoryPlayerInfo();
        GUILayout.Space(10);
        
        DrawContextInfo();
        GUILayout.Space(10);
        
        DrawControlButtons();
        GUILayout.Space(10);
        
        DrawExecutionLog();
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    private void DrawStoryPlayerInfo()
    {
        GUILayout.Label("=== 스토리 플레이어 상태 ===", GUI.skin.GetStyle("label"));
        
        if (StoryPlayer.Instance != null)
        {
            var player = StoryPlayer.Instance;
            GUILayout.Label($"재생 상태: {player.CurrentPlaybackState}");
            
            // 현재 챕터 정보는 private이므로 리플렉션을 사용하거나 public 프로퍼티를 추가해야 함
            // 여기서는 간단히 상태만 표시
        }
        else
        {
            GUILayout.Label("StoryPlayer 인스턴스가 없습니다.");
        }
    }
    
    private void DrawContextInfo()
    {
        GUILayout.Label("=== 컨텍스트 정보 ===", GUI.skin.GetStyle("label"));
        
        GUILayout.Label($"현재 맵: {(StoryPlayerContext.CurrentMap != null ? StoryPlayerContext.CurrentMap.name : "없음")}");
        GUILayout.Label($"플레이어: {(StoryPlayerContext.Player != null ? StoryPlayerContext.Player.name : "없음")}");
        GUILayout.Label($"활성 적: {StoryPlayerContext.ActiveEnemies.Count}마리");
        
        // 변수 목록 (간단히 개수만 표시, 자세한 내용은 로그에서 확인)
        if (GUILayout.Button("컨텍스트 상태 로그 출력"))
        {
            StoryPlayerContext.LogCurrentState();
        }
    }
    
    private void DrawControlButtons()
    {
        GUILayout.Label("=== 제어 ===", GUI.skin.GetStyle("label"));
        
        if (StoryPlayer.Instance != null)
        {
            var player = StoryPlayer.Instance;
            
            GUILayout.BeginHorizontal();
            
            if (player.CurrentPlaybackState == StoryPlayer.PlaybackState.Playing)
            {
                if (GUILayout.Button("일시정지"))
                {
                    player.Pause();
                }
                
                if (GUILayout.Button("다음 스텝"))
                {
                    player.SkipCurrentStep();
                }
            }
            else if (player.CurrentPlaybackState == StoryPlayer.PlaybackState.Paused)
            {
                if (GUILayout.Button("재개"))
                {
                    player.Resume();
                }
            }
            
            if (GUILayout.Button("이전 스텝"))
            {
                player.GoToPreviousStep();
            }
            
            GUILayout.EndHorizontal();
        }
        
        if (GUILayout.Button("실행 로그 지우기"))
        {
            _executionLog.Clear();
        }
    }
    
    private void DrawExecutionLog()
    {
        GUILayout.Label("=== 실행 로그 ===", GUI.skin.GetStyle("label"));
        
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(200));
        
        foreach (var logEntry in _executionLog)
        {
            GUILayout.Label(logEntry, GUI.skin.GetStyle("label"));
        }
        
        GUILayout.EndScrollView();
    }
    
    private void AddLogEntry(string message)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        string logEntry = $"[{timestamp}] {message}";
        
        _executionLog.Add(logEntry);
        
        // 로그 개수 제한
        if (_executionLog.Count > MAX_LOG_ENTRIES)
        {
            _executionLog.RemoveAt(0);
        }
        
        // 파일 저장 (옵션)
        if (saveLogToFile)
        {
            System.IO.File.AppendAllText("story_debug.log", logEntry + "\n");
        }
        
        // 스크롤을 맨 아래로
        _scrollPosition.y = float.MaxValue;
    }
    
    #region 이벤트 핸들러
    
    private void OnStepChanged(StoryStepSO step, int stepIndex)
    {
        AddLogEntry($"스텝 변경: [{stepIndex}] {step.name} - {step.description}");
    }
    
    private void OnPauseStateChanged(bool isPaused)
    {
        AddLogEntry($"일시정지 상태 변경: {(isPaused ? "일시정지됨" : "재개됨")}");
    }
    
    private void OnChapterComplete()
    {
        AddLogEntry("챕터 완료");
    }
    
    private void OnVariableChanged(string name, object oldValue, object newValue)
    {
        AddLogEntry($"변수 변경: {name} = {oldValue} → {newValue}");
    }
    
    private void OnObjectChanged(string name, GameObject obj, bool isRegistered)
    {
        string action = isRegistered ? "등록" : "해제";
        string objName = obj != null ? obj.name : "null";
        AddLogEntry($"오브젝트 {action}: {name} ({objName})");
    }
    
    #endregion
}