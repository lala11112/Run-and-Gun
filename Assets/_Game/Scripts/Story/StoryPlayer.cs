using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// StoryChapterSO를 받아 순서대로 스텝을 실행하는 스토리 재생 엔진입니다.
/// </summary>
public class StoryPlayer : MonoBehaviour
{
    public static StoryPlayer Instance { get; private set; }

    public event Action OnChapterComplete;

    private StoryChapterSO _currentChapter;
    private int _currentStepIndex = -1;
    private IStoryStepState _currentState;

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
    public void Play(StoryChapterSO chapter)
    {
        if (chapter == null || chapter.steps.Count == 0)
        {
            Debug.LogWarning("재생할 챕터가 비어있습니다.");
            CompleteChapter();
            return;
        }

        Debug.Log($"[StoryPlayer] 챕터 '{chapter.name}' 재생 시작.");
        StoryPlayerContext.Reset(); // 컨텍스트 리셋
        _currentChapter = chapter;
        _currentStepIndex = -1;
        
        AdvanceToNextStep();
    }

    /// <summary>
    /// 현재 스텝을 종료하고 다음 스텝으로 넘어갑니다.
    /// 모든 IStoryStepState 구현체는 자신의 작업이 끝나면 이 메서드를 호출해야 합니다.
    /// </summary>
    public void AdvanceToNextStep()
    {
        // 이전 상태 정리
        _currentState?.Exit();
        _currentState = null;

        _currentStepIndex++;

        if (_currentChapter == null || _currentStepIndex >= _currentChapter.steps.Count)
        {
            CompleteChapter();
            return;
        }

        var nextStepData = _currentChapter.steps[_currentStepIndex];
        if (nextStepData != null)
        {
            _currentState = nextStepData.CreateState(this);
            _currentState.Enter();
        }
        else
        {
            Debug.LogWarning($"[StoryPlayer] {_currentStepIndex}번째 스텝이 비어있어 건너뜁니다.");
            AdvanceToNextStep();
        }
    }

    private void Update()
    {
        _currentState?.Tick();
    }
    
    private void CompleteChapter()
    {
        Debug.Log($"[StoryPlayer] 챕터 '{_currentChapter?.name}' 재생 완료.");
        _currentChapter = null;
        OnChapterComplete?.Invoke();
    }
}
