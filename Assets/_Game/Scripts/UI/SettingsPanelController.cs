using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 설정창 전체를 제어하고 탭 전환 및 공통 버튼 기능을 담당합니다.
/// </summary>
public class SettingsPanelController : MonoBehaviour
{
    [Header("탭 버튼")]
    public Button displayTabButton;
    public Button soundTabButton;
    public Button gameplayTabButton;

    [Header("탭 패널")]
    public GameObject displayPanel;
    public GameObject soundPanel;
    public GameObject gameplayPanel;

    [Header("공통 버튼")]
    public Button resetButton;
    public Button applyButton;
    public Button cancelButton;
    public Button confirmButton;
    
    private void OnEnable()
    {
        // 설정 창이 열릴 때 SettingsManager에 임시 데이터 생성을 요청합니다.
        SettingsManager.Instance.OpenSettings();
        // 각 탭 UI에 현재 설정값을 채우도록 알립니다.
        BroadcastMessage("UpdateUI", SendMessageOptions.DontRequireReceiver);
    }
    
    private void Awake()
    {
        // 탭 버튼 이벤트 연결
        displayTabButton.onClick.AddListener(() => OpenTab(displayPanel));
        soundTabButton.onClick.AddListener(() => OpenTab(soundPanel));
        gameplayTabButton.onClick.AddListener(() => OpenTab(gameplayPanel));

        // 공통 버튼 이벤트 연결
        resetButton.onClick.AddListener(OnReset);
        applyButton.onClick.AddListener(OnApply);
        cancelButton.onClick.AddListener(OnCancel);
        confirmButton.onClick.AddListener(OnConfirm);

        // 초기 상태: 디스플레이 탭 열기
        OpenTab(displayPanel);
    }

    private void OpenTab(GameObject activePanel)
    {
        displayPanel.SetActive(activePanel == displayPanel);
        soundPanel.SetActive(activePanel == soundPanel);
        gameplayPanel.SetActive(activePanel == gameplayPanel);
    }

    private void OnReset()
    {
        SettingsManager.Instance.ResetToDefault();
        BroadcastMessage("UpdateUI", SendMessageOptions.DontRequireReceiver);
    }

    private void OnApply()
    {
        SettingsManager.Instance.ApplySettings();
    }

    private void OnCancel()
    {
        // 변경사항 없이 창 닫기
        UIManager.Instance.Pop();
    }

    private void OnConfirm()
    {
        // 변경사항 적용 후 창 닫기
        OnApply();
        OnCancel();
    }
} 