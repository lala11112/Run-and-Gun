using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 플레이어 체력 관리 및 피격 시 처리.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerHealth : MonoBehaviour
{
    [Tooltip("플레이어 최대 체력")] public int maxHealth = 5;
    [Tooltip("피격 후 무적 시간(초)")] public float invincibilityDuration = 0.5f;

    [Header("Camera Shake on Hit")] public float shakeDuration = 0.15f; public float shakeMagnitude = 0.25f;

    private int _currentHealth;
    private float _invincibleTimer;

    public int CurrentHealth => _currentHealth;

    public System.Action<int, int> OnHealthChanged; // current, max
    public System.Action OnPlayerDied;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    private void Update()
    {
        if (_invincibleTimer > 0f)
            _invincibleTimer -= Time.deltaTime;
    }

    public void TakeDamage(int dmg)
    {
        if (_invincibleTimer > 0f) return;

        _currentHealth -= dmg;
        if (_currentHealth < 0) _currentHealth = 0;
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);

        _invincibleTimer = invincibilityDuration;

        CameraShake.Instance?.Shake(shakeDuration, shakeMagnitude);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnPlayerDied?.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
} 