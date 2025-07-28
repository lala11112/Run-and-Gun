using System;
using UnityEngine;

/// <summary>
/// 스토리 모드 – 고정 스테이지 & 컷씬 진행.
/// 현재는 로그만 출력하는 기본 골격.
/// </summary>
public class StoryMode : IGameMode
{
    public event Action<bool> OnRunEnded;

    public GameObject PauseMenuPrefab => Resources.Load<GameObject>("UI/PauseMenu_Story");

    public void Initialize()
    {
        Debug.Log("[StoryMode] Initialize");
        StartRun();
    }

    public PauseMenuContext GetPauseMenuContext()
    {
        // TODO: 실제 데이터로 채우기
        return new StoryPauseContext
        {
            currentObjective = "어머니의 방에서 단서를 찾아보자",
            equippedSkills = SkillManager.Instance?.skillConfigs
        };
    }

    public void StartRun()
    {
        Debug.Log("[StoryMode] StartRun (Chapter Start)");
    }

    public void EndRun(bool victory)
    {
        Debug.Log($"[StoryMode] EndRun – Victory:{victory}");
        OnRunEnded?.Invoke(victory);
    }

    public void Cleanup()
    {
        Debug.Log("[StoryMode] Cleanup");
    }
} 