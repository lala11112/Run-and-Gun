using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UIManager – HUD, 팝업 등을 계층적으로 관리하는 싱글톤.
/// GameplayHUD(HealthHUD, StyleHUD 등)는 Gameplay 상태일 때 자동으로 On/Off 됩니다.
/// 팝업/메뉴는 Push()/Pop() 스택으로 관리해 중복 UI를 방지합니다.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private readonly Stack<GameObject> _uiStack = new();

    private CanvasGroup _gameplayHudGroup;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

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
        if (panel == null) return;
        if (_uiStack.Count > 0)
        {
            var top = _uiStack.Peek();
            top.SetActive(false);
        }
        panel.SetActive(true);
        _uiStack.Push(panel);
    }

    /// <summary>현재 최상단 UI Pop(숨기기)</summary>
    public void Pop()
    {
        if (_uiStack.Count == 0) return;
        var top = _uiStack.Pop();
        top.SetActive(false);
        if (_uiStack.Count > 0)
        {
            _uiStack.Peek().SetActive(true);
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
            if (panel != null)
            {
                panel.SetActive(false);
            }
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
} 