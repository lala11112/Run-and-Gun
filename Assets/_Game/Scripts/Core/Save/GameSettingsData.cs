using System;
using UnityEngine;

/// <summary>
/// 게임의 모든 설정값을 저장하는 직렬화 가능한 클래스입니다.
/// SaveData에 포함되어 저장됩니다.
/// </summary>
[Serializable]
public class GameSettingsData
{
    // 디스플레이
    public int resolutionIndex;
    public FullScreenMode screenMode = FullScreenMode.FullScreenWindow;
    public bool vsync = true;
    public int qualityIndex = -1; // -1은 현재 품질 레벨 사용을 의미

    // 사운드
    [Range(0.0001f, 1f)] public float masterVolume = 1f;
    [Range(0.0001f, 1f)] public float bgmVolume = 1f;
    [Range(0.0001f, 1f)] public float sfxVolume = 1f;
    public bool muteAll = false;

    // 게임플레이
    public bool screenShake = true;
    public bool showDamageNumbers = true;

    /// <summary>
    /// 기본값으로 모든 설정을 되돌립니다.
    /// </summary>
    public void ResetToDefault()
    {
        resolutionIndex = -1; // 현재 해상도를 의미
        screenMode = FullScreenMode.FullScreenWindow;
        vsync = true;
        qualityIndex = QualitySettings.GetQualityLevel();

        masterVolume = 1f;
        bgmVolume = 1f;
        sfxVolume = 1f;
        muteAll = false;

        screenShake = true;
        showDamageNumbers = true;
    }
} 