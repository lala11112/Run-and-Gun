using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 타이틀 화면의 메뉴 전체를 관리하고, 게임 상태에 따라 버튼을 활성화/비활성화합니다.
/// </summary>
public class TitleMenuController : MonoBehaviour
{
    [Header("시작 챕터 설정")]
    [Tooltip("'새 게임'을 눌렀을 때 시작할 스토리 챕터 에셋")]
    public StoryChapterSO startingChapter;
    
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
        // bool hasSaveData = SaveService.Data != null && SaveService.Data.highestFloorCleared > 0;
        if (continueButton != null)
        {
            continueButton.interactable = false; // 이어하기는 아직 미구현
        }
        
        // 로그라이크 모드는 아직 미구현
        if (roguelikeButton != null)
        {
            roguelikeButton.interactable = false;
        }
    }

    private void OnContinue()
    {
        // TODO: 마지막 저장 지점에서 이어하기 로직
        Debug.Log("이어하기 선택됨 (미구현)");
    }

    private void OnNewGame()
    {
        Debug.Log("'새 게임' 선택됨");
        if (startingChapter == null)
        {
            Debug.LogError("'startingChapter'가 지정되지 않았습니다! TitleMenuController의 인스펙터에서 설정해주세요.");
            return;
        }

        if (StoryPlayer.Instance != null)
        {
            // GameManager를 통해 게임 상태를 InGame으로 변경하고,
            // StoryPlayer를 통해 지정된 챕터를 재생합니다.
            GameManager.Instance.ChangeState(GameState.InGame);
            StoryPlayer.Instance.Play(startingChapter);
        }
        else
        {
            Debug.LogError("씬에 StoryPlayer가 존재하지 않습니다! InGame 씬에 StoryPlayer 컴포넌트를 가진 오브젝트를 추가해주세요.");
        }
    }

    private void OnRoguelike()
    {
        Debug.Log("로그라이크 모드 선택됨 (미구현)");
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