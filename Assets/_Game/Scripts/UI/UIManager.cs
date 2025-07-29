using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// UI 스택 관리, HUD 표시/숨김 등 UI 관련 전반을 담당하는 싱글톤 매니저
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private readonly Stack<GameObject> _uiStack = new();

    private CanvasGroup _gameplayHudGroup;
    private Canvas _popupCanvas;
    private const int POPUP_CANVAS_SORT_ORDER = 100;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsurePopupCanvas();

        // 이벤트 구독
        GameEvents.StateChanged += HandleStateChanged;
        SceneLoader.OnLoadCompleted += _ => RefreshHUDReference();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            GameEvents.StateChanged -= HandleStateChanged;
            SceneLoader.OnLoadCompleted -= _ => RefreshHUDReference();
            Instance = null;
        }
    }

    // --------------------- Public API ---------------------
    /// <summary>GameplayHUD 활성/비활성</summary>
    public void ShowGameplayHUD(bool show)
    {
        if (_gameplayHudGroup == null) RefreshHUDReference();
        if (_gameplayHudGroup != null)
        {
            _gameplayHudGroup.alpha = show ? 1f : 0f;
            _gameplayHudGroup.blocksRaycasts = show;
            _gameplayHudGroup.interactable = show;
        }
    }

    /// <summary>팝업/메뉴 GameObject를 스택에 Push(보여주기)</summary>
    public void Push(GameObject panel)
    {
        EnsurePopupCanvas();
        if (panel == null) return;
        if (_uiStack.Count > 0)
        {
            var top = _uiStack.Peek();
            // SetActive(false) 대신 CanvasGroup으로 제어하여 애니메이션 등을 유지할 수 있게 함
            top.GetComponent<CanvasGroup>().interactable = false;
        }
        panel.transform.SetParent(_popupCanvas.transform, false);
        _uiStack.Push(panel);
    }

    /// <summary>현재 최상단 UI Pop(숨기기)</summary>
    public void Pop()
    {
        if (_uiStack.Count == 0) return;
        var top = _uiStack.Pop();
        // 실제 파괴는 각 상태의 Exit에서 처리하므로 여기서는 비활성화만.
        // 오브젝트가 이미 파괴되었을 수 있음
        if (top != null) Destroy(top);

        if (_uiStack.Count > 0)
        {
            _uiStack.Peek().GetComponent<CanvasGroup>().interactable = true;
        }
    }

    /// <summary>
    /// 스택에 있는 모든 UI 팝업을 닫습니다.
    /// </summary>
    public void CloseAllPopups()
    {
        while (_uiStack.Count > 0)
        {
            var panel = _uiStack.Pop();
            // 오브젝트가 이미 파괴되었을 수 있으므로 null 체크
            if (panel != null) Destroy(panel);
        }
    }

    // --------------------- Internal ---------------------
    private void HandleStateChanged(IState st)
    {
        bool showHud = st is GameplayState || st is PausedState; // Pause 중에도 HUD 노출
        ShowGameplayHUD(showHud);
    }

    private void RefreshHUDReference()
    {
        // GameplayHUD 루트는 태그를 붙여 두거나 이름으로 찾는다.
        var hudObj = GameObject.FindWithTag("GameplayHUD") ?? GameObject.Find("GameplayHUD");
        if (hudObj != null)
        {
            _gameplayHudGroup = hudObj.GetComponent<CanvasGroup>();
            if (_gameplayHudGroup == null)
            {
                _gameplayHudGroup = hudObj.AddComponent<CanvasGroup>();
            }
        }
        else
        {
            _gameplayHudGroup = null;
        }
    }

    private void EnsurePopupCanvas()
    {
        if (_popupCanvas != null) return;

        var go = new GameObject("PopupCanvas");
        go.transform.SetParent(this.transform);
        _popupCanvas = go.AddComponent<Canvas>();
        _popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _popupCanvas.sortingOrder = POPUP_CANVAS_SORT_ORDER;

        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();

        DontDestroyOnLoad(go);
    }
} 