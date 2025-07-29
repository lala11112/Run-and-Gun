using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// 디스플레이 설정 탭의 UI를 관리합니다.
/// </summary>
public class DisplaySettingsTab : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown screenModeDropdown;
    public Toggle vsyncToggle;
    public TMP_Dropdown qualityDropdown;

    private Resolution[] _resolutions;

    private void Awake()
    {
        // UI 이벤트 리스너 연결
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);
        vsyncToggle.onValueChanged.AddListener(OnVsyncChanged);
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
    }

    /// <summary>
    /// SettingsPanelController에 의해 호출되어 UI를 현재 설정값으로 채웁니다.
    /// </summary>
    public void UpdateUI()
    {
        // 해상도 드롭다운 채우기
        _resolutions = Screen.resolutions.Select(r => new Resolution { width = r.width, height = r.height }).Distinct().ToArray();
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(_resolutions.Select(r => $"{r.width} x {r.height}").ToList());
        resolutionDropdown.value = SettingsManager.Instance.TempSettings.resolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // 화면 모드
        screenModeDropdown.value = (int)SettingsManager.Instance.TempSettings.screenMode;
        
        // 수직 동기화
        vsyncToggle.isOn = SettingsManager.Instance.TempSettings.vsync;
        
        // 그래픽 품질
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(QualitySettings.names.ToList());
        qualityDropdown.value = SettingsManager.Instance.TempSettings.qualityIndex;
    }

    // --- UI 값 변경 시 TempSettings 업데이트 ---
    private void OnResolutionChanged(int index) => SettingsManager.Instance.TempSettings.resolutionIndex = index;
    private void OnScreenModeChanged(int index) => SettingsManager.Instance.TempSettings.screenMode = (FullScreenMode)index;
    private void OnVsyncChanged(bool value) => SettingsManager.Instance.TempSettings.vsync = value;
    private void OnQualityChanged(int index) => SettingsManager.Instance.TempSettings.qualityIndex = index;
} 