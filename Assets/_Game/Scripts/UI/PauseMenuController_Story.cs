using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 스토리 모드용 일시정지 메뉴 컨트롤러입니다.
/// </summary>
public class PauseMenuController_Story : PauseMenuControllerBase
{
    [Header("스토리 전용 UI")]
    public Button restartButton;
    public TextMeshProUGUI objectiveText;
    // TODO: 스킬 아이콘을 표시할 UI 루트 추가

    protected override void Awake()
    {
        base.Awake();
        restartButton?.onClick.AddListener(OnRestart);
    }

    public override void Initialize(PauseMenuContext context)
    {
        if (context is StoryPauseContext storyContext)
        {
            objectiveText.text = $"현재 목표: {storyContext.currentObjective}";
            // TODO: 스킬 아이콘 목록을 동적으로 생성하여 표시
        }
    }

    private void OnRestart()
    {
        Debug.Log("마지막 체크포인트에서 재시작 (미구현)");
        // 현재 씬 재시작 또는 마지막 체크포인트 씬 로드
    }
} 