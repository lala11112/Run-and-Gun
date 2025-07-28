using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 로그라이크 모드용 일시정지 메뉴 컨트롤러입니다.
/// </summary>
public class PauseMenuController_Roguelike : PauseMenuControllerBase
{
    [Header("로그라이크 전용 UI")]
    public Button giveUpButton;
    public TextMeshProUGUI floorText;
    public TextMeshProUGUI playtimeText;
    public TextMeshProUGUI willpowerText;
    // TODO: 스킬 및 아이템 아이콘을 표시할 UI 루트 추가

    protected override void Awake()
    {
        base.Awake();
        giveUpButton?.onClick.AddListener(OnGiveUp);
    }

    public override void Initialize(PauseMenuContext context)
    {
        if (context is RoguelikePauseContext roguelikeContext)
        {
            floorText.text = $"현재 층: {roguelikeContext.currentFloor}F";
            playtimeText.text = $"플레이 시간: {roguelikeContext.playtime:F0}초";
            willpowerText.text = $"획득한 의지: {roguelikeContext.willpowerEarned}";
            // TODO: 스킬, 아이템 아이콘 목록을 동적으로 생성하여 표시
        }
    }

    private void OnGiveUp()
    {
        Debug.Log("포기하고 마을로 돌아가기 (미구현)");
        // 확인 팝업 후, 결과 정산 및 '주인공의 방' 씬으로 전환
    }
} 