using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 타이틀 화면에서 게임 모드 선택 버튼을 처리합니다.
/// </summary>
public class TitleMenuUI : MonoBehaviour
{
    [Tooltip("로그라이크 모드 버튼")] public Button roguelikeButton;
    [Tooltip("스토리 모드 버튼")] public Button storyButton;

    private void Awake()
    {
        if (roguelikeButton != null)
            roguelikeButton.onClick.AddListener(() => GameManager.Instance.SwitchMode(GameModeType.Roguelike));

        if (storyButton != null)
            storyButton.onClick.AddListener(() => GameManager.Instance.SwitchMode(GameModeType.Story));
    }
} 