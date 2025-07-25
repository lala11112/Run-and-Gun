using UnityEngine;
using DG.Tweening;
using TMPro;
using PixelCrushers.DialogueSystem;

/// <summary>
/// TestBattleManager의 라운드 수를 화면에 표시하고, 라운드가 증가할 때마다 팝업 애니메이션을 보여주는 HUD.
/// </summary>
public class RoundHUD : MonoBehaviour
{
    [Header("UI 참조")]
    [Tooltip("라운드 번호를 표시할 TextMeshProUGUI")] public TextMeshProUGUI roundText;
    [Tooltip("라운드 팝업용 CanvasGroup (알파 트윈)")] public CanvasGroup canvasGroup;

    [Header("애니메이션 설정")]
    [Tooltip("페이드 인/아웃 시간(초)")] public float fadeTime = 0.25f;
    [Tooltip("팝업이 화면에 유지되는 시간(초)")] public float showDuration = 1.0f;
    [Tooltip("팝업 크기 확대 비율")] public float punchScale = 1.2f;

    private TestBattleManager _tbm;
    private int _cachedRound = -1;
    private Tween _fadeTween;

    private void Start()
    {
        _tbm = FindObjectOfType<TestBattleManager>();
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        UpdateRoundImmediate();
    }

    private void OnDestroy()
    {
        _fadeTween?.Kill();
    }

    private void Update()
    {
        if (_tbm == null) return;

        // 대화창이 열려 있으면 HUD 숨김
        if (DialogueManager.IsConversationActive)
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            return; // 라운드 변경 감지 생략
        }

        if (_tbm.currentRound != _cachedRound)
        {
            ShowRound(_tbm.currentRound);
        }
    }

    private void UpdateRoundImmediate()
    {
        if (roundText != null && _tbm != null)
        {
            roundText.text = $"Round {_tbm.currentRound}";
        }
    }

    private void ShowRound(int round)
    {
        _cachedRound = round;
        if (roundText != null)
        {
            roundText.text = $"Round {round}";
            // 펀치 스케일 효과
            roundText.transform.localScale = Vector3.one;
            roundText.transform.DOPunchScale(Vector3.one * (punchScale - 1f), fadeTime, 1, 0f).SetLink(roundText.gameObject);
        }

        if (canvasGroup != null)
        {
            _fadeTween?.Kill();
            _fadeTween = canvasGroup.DOFade(1f, fadeTime).SetLink(canvasGroup.gameObject)
                .OnComplete(() =>
                {
                    _fadeTween = canvasGroup.DOFade(0f, fadeTime).SetDelay(showDuration).SetLink(canvasGroup.gameObject);
                });
        }
    }
} 