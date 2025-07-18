using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 스킬(X/Z/C/V) 쿨타임을 아이콘 위에 표시하고, Image(Radial360) fillAmount 로 게이지를 시각화.
/// </summary>
public class SkillCooldownDisplay : MonoBehaviour
{
    [Tooltip("표시할 스킬 타입 (Z/X/C/V)")] public SkillType skillType;
    [Tooltip("쿨타임을 표시할 TextMeshProUGUI")] public TextMeshProUGUI cooldownText;
    [Tooltip("쿨타임 게이지로 사용할 Slider (Fill Rect 이미지)")] public Slider cooldownSlider;

    private void Update()
    {
        if (SkillManager.Instance == null) return;

        float remaining = SkillManager.Instance.GetCooldownRemaining(skillType);
        float total = SkillManager.Instance.GetTotalCooldown(skillType);

        if (cooldownSlider != null)
        {
            cooldownSlider.maxValue = total;
            cooldownSlider.value = remaining;
        }

        if (remaining <= 0.01f || total <= 0.01f)
        {
            if (cooldownText != null) cooldownText.text = "";
            return;
        }

        if (cooldownText != null) cooldownText.text = Mathf.CeilToInt(remaining).ToString();
    }
} 