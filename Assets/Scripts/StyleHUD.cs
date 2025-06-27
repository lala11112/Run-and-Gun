using UnityEngine;
using TMPro;

/// <summary>
/// StyleManager의 현재 점수와 랭크를 화면 UI(TextMeshPro)로 표시하는 간단한 HUD 스크립트.
/// Canvas > TextMeshProUGUI 두 개를 연결하고, 본 스크립트를 Canvas 상에 부착하면 된다.
/// </summary>
public class StyleHUD : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("점수를 표시할 TextMeshProUGUI")] public TextMeshProUGUI scoreText;
    [Tooltip("랭크를 표시할 TextMeshProUGUI")] public TextMeshProUGUI rankText;

    private void Update()
    {
        if (StyleManager.Instance == null) return;

        // 점수 표시
        if (scoreText != null)
        {
            scoreText.text = $"Score: {StyleManager.Instance.CurrentScore}";
        }

        // 랭크 표시 + 색상
        if (rankText != null)
        {
            StyleRank rank = StyleManager.Instance.CurrentRank;
            rankText.text = $"Rank: {rank}";
            rankText.color = GetColorForRank(rank);
        }
    }

    private Color GetColorForRank(StyleRank rank)
    {
        return rank switch
        {
            StyleRank.D => new Color(0.5f,0.5f,0.5f),
            StyleRank.C => new Color(0.3f, 0.9f, 0.3f),
            StyleRank.B => Color.cyan,
            StyleRank.A => new Color(0.9f, 0.4f, 1f),
            _ => Color.white,
        };
    }
} 