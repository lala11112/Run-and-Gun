using System;
using UnityEngine;

/// <summary>
/// 게임 진행도와 옵션을 저장하는 직렬화 가능한 클래스입니다.
/// </summary>
[Serializable]
public class SaveData
{
    [Header("진행도")] public int highestFloorCleared = 0;
    public int storyChapterUnlocked = 0;

    [Header("보유 재화")]
    public int gold = 0;

    [Header("구매/업그레이드 기록")] public string[] purchasedUpgrades = Array.Empty<string>();

    [Header("옵션")]
    [Range(0f,1f)] public float masterVolume = 1f;
    [Range(0f,1f)] public float bgmVolume = 1f;
    [Range(0f,1f)] public float sfxVolume = 1f;
    public GameSettingsData settings = new GameSettingsData();
} 