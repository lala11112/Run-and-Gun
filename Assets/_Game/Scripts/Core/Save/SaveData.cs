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

    [Header("옵션")]
    [Range(0f,1f)] public float masterVolume = 1f;
    [Range(0f,1f)] public float bgmVolume = 1f;
    [Range(0f,1f)] public float sfxVolume = 1f;

    // 필요한 데이터 항목을 자유롭게 확장
} 