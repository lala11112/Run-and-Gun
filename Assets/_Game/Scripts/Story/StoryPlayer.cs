using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// StoryChapterSO를 받아 순서대로 스텝을 실행하는 스토리 재생 엔진입니다.
/// 일시정지, 재시작, 건너뛰기, 되돌아가기 등의 고급 기능을 지원합니다.
/// </summary>
public class StoryPlayer : MonoBehaviour
{
    public static StoryPlayer Instance { get; private set; }

    public event Action OnChapterComplete;
    public event Action<StoryStepSO, int> OnStepChanged; // 스텝 변경 이벤트
    public event Action<bool> OnPauseStateChanged; // 일시정지 상태 변경 이벤트

    [Header("디버그 설정")]
    [Tooltip("스텝 실행 상세 로그를 출력할지 여부")]
    public bool enableDetailedLogging = true;
    
    [Tooltip("스텝 실행 타임아웃 시간 (초, 0이면 무제한)")]
    public float stepTimeout = 30f;

    // 상태 관리
    public enum PlaybackState { Stopped, Playing, Paused }
    public PlaybackState CurrentPlaybackState { get; private set; } = PlaybackState.Stopped;
    
    private StoryChapterSO _currentChapter;
    private int _currentStepIndex = -1;
    private IStoryStepState _currentState;
    
    // 고급 기능
    private Coroutine _timeoutCoroutine;
    private List<int> _stepHistory = new List<int>(); // 되돌아가기를 위한 히스토리

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("StoryPlayer의 인스턴스가 이미 존재합니다. 새로 생성된 인스턴스를 파괴합니다.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // 이 오브젝트가 씬 전환 시 파괴되지 않도록 설정합니다.
        // 이것이 씬 경계를 넘나드는 스토리 진행의 핵심입니다.
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 지정된 챕터의 재생을 시작합니다.
    /// </summary>
    public void Play(StoryChapterSO chapter, int startStepIndex = 0)
    {
        if (chapter == null || chapter.steps.Count == 0)
        {
            LogWarning("재생할 챕터가 비어있습니다.");
            CompleteChapter();
            return;
        }

        LogInfo($"챕터 '{chapter.name}' 재생 시작 (스텝 {startStepIndex}부터)");
        
        // 상태 초기화
        StoryPlayerContext.Reset();
        _currentChapter = chapter;
        _currentStepIndex = startStepIndex - 1; // AdvanceToNextStep에서 +1 되므로
        _stepHistory.Clear();
        CurrentPlaybackState = PlaybackState.Playing;
        
        AdvanceToNextStep();
    }
    
    /// <summary>
    /// 스토리 재생을 일시정지합니다.
    /// </summary>
    public void Pause()
    {
        if (CurrentPlaybackState != PlaybackState.Playing) return;
        
        CurrentPlaybackState = PlaybackState.Paused;
        LogInfo("스토리 재생이 일시정지되었습니다.");
        OnPauseStateChanged?.Invoke(true);
    }
    
    /// <summary>
    /// 일시정지된 스토리 재생을 재개합니다.
    /// </summary>
    public void Resume()
    {
        if (CurrentPlaybackState != PlaybackState.Paused) return;
        
        CurrentPlaybackState = PlaybackState.Playing;
        LogInfo("스토리 재생이 재개되었습니다.");
        OnPauseStateChanged?.Invoke(false);
    }
    
    /// <summary>
    /// 현재 스텝을 건너뛰고 다음 스텝으로 이동합니다.
    /// </summary>
    public void SkipCurrentStep()
    {
        if (CurrentPlaybackState != PlaybackState.Playing) return;
        
        LogInfo($"스텝 {_currentStepIndex} 건너뛰기");
        AdvanceToNextStep();
    }
    
    /// <summary>
    /// 이전 스텝으로 되돌아갑니다.
    /// </summary>
    public bool GoToPreviousStep()
    {
        if (_stepHistory.Count == 0) return false;
        
        int previousStepIndex = _stepHistory[_stepHistory.Count - 1];
        _stepHistory.RemoveAt(_stepHistory.Count - 1);
        
        LogInfo($"이전 스텝 {previousStepIndex}로 되돌아가기");
        GoToStep(previousStepIndex, false); // 히스토리에 추가하지 않음
        return true;
    }
    
    /// <summary>
    /// 특정 스텝으로 직접 이동합니다.
    /// </summary>
    public bool GoToStep(int stepIndex, bool addToHistory = true)
    {
        if (_currentChapter == null || stepIndex < 0 || stepIndex >= _currentChapter.steps.Count)
        {
            LogWarning($"유효하지 않은 스텝 인덱스: {stepIndex}");
            return false;
        }
        
        // 현재 스텝을 히스토리에 추가
        if (addToHistory && _currentStepIndex >= 0)
        {
            _stepHistory.Add(_currentStepIndex);
        }
        
        // 현재 상태 정리
        StopTimeoutCoroutine();
        _currentState?.Exit();
        _currentState = null;
        
        // 새 스텝으로 이동
        _currentStepIndex = stepIndex;
        ExecuteCurrentStep();
        
        return true;
    }

    /// <summary>
    /// 현재 스텝을 종료하고 다음 스텝으로 넘어갑니다.
    /// 모든 IStoryStepState 구현체는 자신의 작업이 끝나면 이 메서드를 호출해야 합니다.
    /// </summary>
    public void AdvanceToNextStep()
    {
        if (CurrentPlaybackState != PlaybackState.Playing) return;
        
        // 현재 스텝을 히스토리에 추가
        if (_currentStepIndex >= 0)
        {
            _stepHistory.Add(_currentStepIndex);
        }
        
        // 상태 정리
        StopTimeoutCoroutine();
        _currentState?.Exit();
        _currentState = null;

        _currentStepIndex++;

        if (_currentChapter == null || _currentStepIndex >= _currentChapter.steps.Count)
        {
            CompleteChapter();
            return;
        }

        ExecuteCurrentStep();
    }
    
    /// <summary>
    /// 현재 스텝을 실행합니다.
    /// </summary>
    private void ExecuteCurrentStep()
    {
        if (_currentChapter == null || _currentStepIndex < 0 || _currentStepIndex >= _currentChapter.steps.Count)
        {
            LogWarning("유효하지 않은 스텝 실행 시도");
            return;
        }
        
        var stepData = _currentChapter.steps[_currentStepIndex];
        if (stepData != null)
        {
            LogInfo($"스텝 {_currentStepIndex} 실행: {stepData.name} ({stepData.description})");
            
            try
            {
                _currentState = stepData.CreateState(this);
                _currentState.Enter();
                
                // 타임아웃 설정
                if (stepTimeout > 0)
                {
                    _timeoutCoroutine = StartCoroutine(StepTimeoutCoroutine());
                }
                
                // 스텝 변경 이벤트 발생
                OnStepChanged?.Invoke(stepData, _currentStepIndex);
            }
            catch (System.Exception e)
            {
                LogError($"스텝 {_currentStepIndex} 실행 중 오류: {e.Message}");
                HandleStepError();
            }
        }
        else
        {
            LogWarning($"{_currentStepIndex}번째 스텝이 비어있어 건너뜁니다.");
            AdvanceToNextStep();
        }
    }
    
    /// <summary>
    /// 스텝 실행 중 오류가 발생했을 때의 처리
    /// </summary>
    private void HandleStepError()
    {
        LogWarning("스텝 오류로 인해 다음 스텝으로 진행합니다.");
        AdvanceToNextStep();
    }
    
    /// <summary>
    /// 스텝 타임아웃 코루틴
    /// </summary>
    private System.Collections.IEnumerator StepTimeoutCoroutine()
    {
        yield return new WaitForSeconds(stepTimeout);
        
        if (_currentState != null)
        {
            LogWarning($"스텝 {_currentStepIndex} 타임아웃 발생. 다음 스텝으로 진행합니다.");
            AdvanceToNextStep();
        }
    }
    
    /// <summary>
    /// 타임아웃 코루틴을 정지합니다.
    /// </summary>
    private void StopTimeoutCoroutine()
    {
        if (_timeoutCoroutine != null)
        {
            StopCoroutine(_timeoutCoroutine);
            _timeoutCoroutine = null;
        }
    }

    private void Update()
    {
        if (CurrentPlaybackState == PlaybackState.Playing)
        {
            _currentState?.Tick();
        }
    }
    
    private void CompleteChapter()
    {
        LogInfo($"챕터 '{_currentChapter?.name}' 재생 완료.");
        
        // 상태 정리
        CurrentPlaybackState = PlaybackState.Stopped;
        StopTimeoutCoroutine();
        _currentState?.Exit();
        _currentState = null;
        _currentChapter = null;
        _stepHistory.Clear();
        
        OnChapterComplete?.Invoke();
    }
    
    // 로깅 유틸리티 메서드들
    private void LogInfo(string message)
    {
        if (enableDetailedLogging)
        {
            Debug.Log($"[StoryPlayer] {message}");
        }
    }
    
    private void LogWarning(string message)
    {
        Debug.LogWarning($"[StoryPlayer] {message}");
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[StoryPlayer] {message}");
    }
    
    private void OnDestroy()
    {
        StopTimeoutCoroutine();
    }
}
