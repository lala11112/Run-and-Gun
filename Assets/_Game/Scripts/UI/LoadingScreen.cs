using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 간단한 로딩 화면 컨트롤러.
/// Resources/LoadingScreen 프리팹을 동적으로 로드하여 ProgressBar 를 업데이트합니다.
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    private static LoadingScreen _instance;

    [Tooltip("로딩 진행률을 표시할 Slider")] public Slider progressSlider;
    [Tooltip("로딩 진행률을 표시할 Text (선택)")] public TMPro.TextMeshProUGUI progressText;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _instance = this;
        _canvasGroup = GetComponent<CanvasGroup>();
        Hide();
    }

    public static void Show()
    {
        EnsureInstance();
        if (_instance == null) return;
        _instance._canvasGroup.alpha = 1f;
        _instance._canvasGroup.blocksRaycasts = true;
    }

    public static void Hide()
    {
        if (_instance == null) return;
        _instance._canvasGroup.alpha = 0f;
        _instance._canvasGroup.blocksRaycasts = false;
    }

    public static void UpdateProgress(float value)
    {
        if (_instance == null) return;
        value = Mathf.Clamp01(value);
        if (_instance.progressSlider != null)
            _instance.progressSlider.value = value;
        if (_instance.progressText != null)
            _instance.progressText.text = $"{value * 100f:0}%";
    }

    private static void EnsureInstance()
    {
        if (_instance != null) return;
        // 프리팹을 GameObject 타입으로 로드한 뒤 컴포넌트를 찾는다.
        var prefabGO = Resources.Load<GameObject>("UI/LoadingScreen");
        if (prefabGO != null)
        {
            var go = Instantiate(prefabGO);
            _instance = go.GetComponent<LoadingScreen>();
            if (_instance == null)
            {
                Debug.LogError("[LoadingScreen] LoadingScreen 프리팹에 LoadingScreen 컴포넌트가 없습니다. 자동으로 추가합니다.");
                _instance = go.AddComponent<LoadingScreen>();
            }
        }
        else
        {
            Debug.LogWarning("[LoadingScreen] Resources/UI/LoadingScreen 프리팹을 찾을 수 없습니다. 경로/이름/Resources 폴더 여부를 확인하세요.");
        }
    }
} 