using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
// using TMPro; // 텍스트 제거로 필요 없으면 삭제 가능

/// <summary>
/// 플레이어 체력을 표시하는 간단한 HUD.
/// </summary>
public class HealthHUD : MonoBehaviour
{
    [Tooltip("HP 표시 Slider (Fill 방식)")] public Slider healthSlider;
    [Tooltip("HUD CanvasGroup (페이드용)")] public CanvasGroup canvasGroup;

    [Header("HUD 페이드 설정")]
    [Tooltip("피격 시 HUD가 나타나는 시간")] public float fadeTime = 0.2f;
    [Tooltip("HUD가 화면에 유지되는 시간")] public float showDuration = 1.5f;

    private PlayerHealth _playerHealth;

    private Tween _fadeTween;
    private void OnDestroy()
    {
        _fadeTween?.Kill();
    }

    private void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null) _playerHealth = player.GetComponent<PlayerHealth>();

        if (_playerHealth != null)
        {
            _playerHealth.OnHealthChanged += HandleHealthChanged;
        }

        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (healthSlider != null) healthSlider.maxValue = _playerHealth != null ? _playerHealth.maxHealth : 1;
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.value = current;
        }

        if (canvasGroup != null)
        {
            _fadeTween?.Kill();
            _fadeTween = canvasGroup.DOFade(1f, fadeTime)
                .SetLink(canvasGroup.gameObject)
                .OnComplete(() =>
                {
                    _fadeTween = canvasGroup.DOFade(0f, fadeTime)
                                       .SetDelay(showDuration)
                                       .SetLink(canvasGroup.gameObject);
                });
        }
    }
} 