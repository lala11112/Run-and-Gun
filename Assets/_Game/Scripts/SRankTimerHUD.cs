using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// S 랭크 지속 시간을 Slider로 표시하는 HUD.
/// StyleManager의 SRemainingNormalized 값을 읽어 Slider.value 에 반영한다.
/// </summary>
public class SRankTimerHUD : MonoBehaviour
{
    [Tooltip("S Rank Timer Slider")] public Slider timerSlider;
    [Tooltip("S 랭크 아닐 때 숨김 여부")] public bool hideWhenInactive = true;

    private void Update()
    {
        if (timerSlider == null || StyleManager.Instance == null) return;

        float t = StyleManager.Instance.SRemainingNormalized;
        timerSlider.value = t;

        if (hideWhenInactive)
        {
            timerSlider.gameObject.SetActive(t > 0f);
        }
    }
} 