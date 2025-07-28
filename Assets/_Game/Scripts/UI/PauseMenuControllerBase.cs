using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 모든 일시정지 메뉴의 기본 기능을 정의하는 추상 클래스입니다.
/// </summary>
public abstract class PauseMenuControllerBase : MonoBehaviour
{
    [Header("공통 버튼")]
    public Button resumeButton;
    public Button settingsButton;
    public Button mainMenuButton;

    protected virtual void Awake()
    {
        resumeButton?.onClick.AddListener(OnResume);
        settingsButton?.onClick.AddListener(OnSettings);
        mainMenuButton?.onClick.AddListener(OnMainMenu);
    }

    /// <summary>
    /// 외부에서 컨텍스트 데이터를 받아 UI를 업데이트합니다.
    /// </summary>
    public abstract void Initialize(PauseMenuContext context);

    protected virtual void OnResume()
    {
        if (GameManager.Instance != null && GameManager.Instance.TryGetState(out PausedState ps))
        {
            ps.Resume();
        }
    }

    protected virtual void OnSettings()
    {
        Debug.Log("설정 창 열기 (미구현)");
        // UIManager.Instance.Push(settingsPanelPrefab);
    }

    protected virtual void OnMainMenu()
    {
        // 경고 팝업 후 메인 메뉴로
        GameManager.Instance?.ReturnToTitle();
    }
} 