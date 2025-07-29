using UnityEngine;

/// <summary>
/// 게임 설정을 관리하고, UI와 실제 시스템 간의 상호작용을 처리하는 싱글톤입니다.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    // 현재 설정의 임시 복사본. UI는 이 데이터를 수정합니다.
    public GameSettingsData TempSettings { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 설정 창이 열릴 때 호출됩니다. 원본 설정의 깊은 복사본을 만듭니다.
    /// </summary>
    public void OpenSettings()
    {
        // SaveService.Data.settings를 복사하여 TempSettings를 만듭니다.
        // JsonUtility를 이용한 간단한 깊은 복사 트릭.
        string json = JsonUtility.ToJson(SaveService.Data.settings);
        TempSettings = JsonUtility.FromJson<GameSettingsData>(json);
    }

    /// <summary>
    /// '적용' 버튼 클릭 시 호출. 임시 설정을 실제 게임과 저장 데이터에 반영합니다.
    /// </summary>
    public void ApplySettings()
    {
        // 1. 실제 게임 시스템에 설정 적용
        // 해상도 및 화면 모드
        Resolution res = Screen.resolutions[TempSettings.resolutionIndex];
        Screen.SetResolution(res.width, res.height, TempSettings.screenMode);
        
        // 수직 동기화
        QualitySettings.vSyncCount = TempSettings.vsync ? 1 : 0;
        
        // 그래픽 품질
        QualitySettings.SetQualityLevel(TempSettings.qualityIndex, true);

        // 오디오 (AudioMixer 필요 - 예시)
        // AudioMixer.SetFloat("MasterVolume", Mathf.Log10(TempSettings.masterVolume) * 20);

        // 2. 원본 저장 데이터에 임시 설정 덮어쓰기
        string json = JsonUtility.ToJson(TempSettings);
        SaveService.Data.settings = JsonUtility.FromJson<GameSettingsData>(json);

        // 3. 파일에 저장
        SaveService.Save();

        Debug.Log("설정이 적용되었습니다.");
    }

    /// <summary>
    /// '기본값으로' 버튼 클릭 시 호출. 임시 설정을 기본값으로 되돌립니다.
    /// </summary>
    public void ResetToDefault()
    {
        TempSettings.ResetToDefault();
    }
} 