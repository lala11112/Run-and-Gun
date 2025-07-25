using UnityEngine;

/// <summary>
/// 일시정지 메뉴 캔버스 토글 – GameManager.OnPauseToggled 이벤트를 구독.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot; // PausePanel

    void Awake()
    {
        GameManager.Instance.OnPauseToggled += HandlePause;
        panelRoot.SetActive(GameManager.Instance.IsPaused);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPauseToggled -= HandlePause;
    }

    private void HandlePause(bool paused)
    {
        panelRoot.SetActive(paused);
    }

    public void OnResumeButton() => GameManager.Instance.SetPause(false);
} 