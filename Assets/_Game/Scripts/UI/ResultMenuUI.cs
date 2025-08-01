using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 결과 화면 메뉴: 다시 시도 / 타이틀로 버튼.
/// </summary>
public class ResultMenuUI : MonoBehaviour
{
    [SerializeField] private Button retryButton;
    [SerializeField] private Button titleButton;

    private void Awake()
    {
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetry);

        if (titleButton != null)
            titleButton.onClick.AddListener(OnTitle);
    }

    private void OnRetry()
    {
        // TODO: 새로운 재시작 로직 구현 필요
        // // 동일 모드 재시작
        // GameManager.Instance.SwitchMode(GameManager.Instance.CurrentState == GameState.GameOver ? GameModeType.Roguelike : GameModeType.Story);
    }

    private void OnTitle()
    {
        GameManager.Instance.ChangeState(GameState.Title);
    }
} 