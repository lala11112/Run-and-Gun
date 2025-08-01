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
        // TODO: 새로운 일시정지 로직 구현 필요
        // // PausedState에게 재개를 직접 요청
        // if (GameManager.Instance != null &&
        //     GameManager.Instance.TryGetState(out PausedState pausedState))
        // {
        //     pausedState.Resume();
        // }
    }

    private void OnTitle()
    {
        // GameManager의 공용 메서드 호출
        GameManager.Instance?.ReturnToTitle();
    }
} 