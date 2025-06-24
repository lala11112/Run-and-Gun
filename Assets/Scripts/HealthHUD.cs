using UnityEngine;
using TMPro;

/// <summary>
/// 플레이어 체력을 표시하는 간단한 HUD.
/// </summary>
public class HealthHUD : MonoBehaviour
{
    [Tooltip("체력을 표시할 TextMeshProUGUI")]
    public TextMeshProUGUI healthText;

    private PlayerHealth _playerHealth;

    private void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null) _playerHealth = player.GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (healthText == null || _playerHealth == null) return;
        healthText.text = $"HP: {_playerHealth.CurrentHealth} / {_playerHealth.maxHealth}";
    }
} 