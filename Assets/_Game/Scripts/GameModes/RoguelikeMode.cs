using System;
using UnityEngine;

/// <summary>
/// 로그라이크 모드 – 라운드/층 반복 구조.
/// 현재는 더미 구현으로 로그만 출력합니다.
/// </summary>
public class RoguelikeMode : IGameMode
{
    public event Action<bool> OnRunEnded;

    public void Initialize()
    {
        Debug.Log("[RoguelikeMode] Initialize");
        // TODO: 맵/플레이어 초기화, 세이브 로드 등
        StartRun();
    }

    public void StartRun()
    {
        Debug.Log("[RoguelikeMode] StartRun");
        // 라운드 루프 시작 / TestBattleManager 연결 예정
    }

    public void EndRun(bool victory)
    {
        Debug.Log($"[RoguelikeMode] EndRun – Victory:{victory}");
        OnRunEnded?.Invoke(victory);
    }

    public void Cleanup()
    {
        Debug.Log("[RoguelikeMode] Cleanup");
    }
} 