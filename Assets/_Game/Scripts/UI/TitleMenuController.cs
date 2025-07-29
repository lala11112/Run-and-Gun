using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 타이틀 화면의 메뉴 전체를 관리하고, 게임 상태에 따라 버튼을 활성화/비활성화합니다.
/// </summary>
public class TitleMenuController : MonoBehaviour
{
    [Header("메뉴 버튼")]
    public Button continueButton;
    public Button newGameButton;
    public Button roguelikeButton;
    public Button settingsButton;
    public Button exitButton;

    private void Start()
    {
        // 버튼에 기능 연결
        continueButton?.onClick.AddListener(OnContinue);
        newGameButton?.onClick.AddListener(OnNewGame);
        roguelikeButton?.onClick.AddListener(OnRoguelike);
        settingsButton?.onClick.AddListener(OnSettings);
        exitButton?.onClick.AddListener(OnExit);

        // 저장 데이터 존재 여부에 따라 '이어하기' 버튼 상태 결정
        bool hasSaveData = SaveService.Data != null && SaveService.Data.highestFloorCleared > 0;
        if (continueButton != null)
        {
            continueButton.interactable = hasSaveData;
        }
        // 로그라이크 모드 해금 조건 (예: 스토리 1챕터 클리어)
        bool roguelikeUnlocked = SaveService.Data != null && SaveService.Data.storyChapterUnlocked > 0;
        if (roguelikeButton != null)
        {
            roguelikeButton.interactable = roguelikeUnlocked;
        }
    }

    private void OnContinue()
    {
        // TODO: 마지막 저장 지점에서 이어하기 로직
        Debug.Log("이어하기 선택됨");
    }

    private void OnNewGame()
    {
        Debug.Log("새 게임 선택됨");
        GameManager.Instance.SwitchMode(GameModeType.Story);
    }

    private void OnRoguelike()
    {
        Debug.Log("로그라이크 모드 선택됨");
        GameManager.Instance.SwitchMode(GameModeType.Roguelike);
    }

    private void OnSettings()
    {
        Debug.Log("설정 선택됨");
        OpenSettingsPanel();
    }

    private void OnExit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OpenSettingsPanel()
    {
        var prefab = Resources.Load<GameObject>("UI/SettingsPanel");
        if (prefab != null)
        {
            var panel = Instantiate(prefab);
            UIManager.Instance.Push(panel);
        }
        else
        {
            Debug.LogError("Resources/UI/SettingsPanel.prefab을 찾을 수 없습니다!");
        }
    }
} 