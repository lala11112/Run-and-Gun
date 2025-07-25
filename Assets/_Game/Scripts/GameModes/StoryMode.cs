using System;
using UnityEngine;

/// <summary>
/// 스토리 모드 – 고정 스테이지 & 컷씬 진행.
/// 현재는 로그만 출력하는 기본 골격.
/// </summary>
public class StoryMode : IGameMode
{
    public event Action<bool> OnRunEnded;

    public void Initialize()
    {
        Debug.Log("[StoryMode] Initialize");
        StartRun();
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