using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pause 메뉴 UI 컨트롤러 – Resume, Title 버튼을 연결합니다.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Tooltip("Resume 버튼")] public Button resumeButton;
    [Tooltip("Title 버튼")] public Button titleButton;

    private void Awake()
    {
        // Prefab에서 연결이 끊어진 경우를 대비해 이름 기반 찾기
        if (resumeButton == null)
            resumeButton = transform.Find("ResumeButton")?.GetComponent<Button>();
        if (titleButton == null)
            titleButton = transform.Find("TitleButton")?.GetComponent<Button>();

        if (resumeButton != null) resumeButton.onClick.AddListener(OnResume);
        if (titleButton != null) titleButton.onClick.AddListener(OnTitle);
    }

    private void OnResume()
    {
        // UIManager 스택 Pop → PausedState.Resume()
        UIManager.Instance?.Pop();
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused)
        {
            // Esc 로직과 동일하게 재개
            // GameManager 내부 _stateMachine 참조에 직접 접근 불가하므로 Input으로 재개
            // 간단히 Esc 키 시뮬레이트
            GameManager.Instance.SendMessage("TogglePause", SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnTitle()
    {
        UIManager.Instance?.Pop();
        GameManager.Instance?.SwitchMode(GameModeType.Story); // 예시: 타이틀로 가기 전 모드 해제
        GameManager.Instance?.ChangeState(GameState.Title);
    }
} 