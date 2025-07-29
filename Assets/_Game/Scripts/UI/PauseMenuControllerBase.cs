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
        var prefab = Resources.Load<GameObject>("UI/SettingsPanel");
        if (prefab != null)
        {
            var panel = Instantiate(prefab);
            // UIManager가 현재 PauseMenu 위에 SettingsPanel을 쌓습니다.
            UIManager.Instance.Push(panel);
        }
        else
        {
            Debug.LogError("Resources/UI/SettingsPanel.prefab을 찾을 수 없습니다!");
        }
    }

    protected virtual void OnMainMenu()
    {
        // 경고 팝업 후 메인 메뉴로
        GameManager.Instance?.ReturnToTitle();
    }
} 