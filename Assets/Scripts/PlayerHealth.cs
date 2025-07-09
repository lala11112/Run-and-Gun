using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

/// <summary>
/// 플레이어 체력 관리 및 피격 시 처리.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerHealth : MonoBehaviour
{
    [Tooltip("플레이어 최대 체력")] public int maxHealth = 5;
    [Tooltip("피격 후 무적 시간(초)")] public float invincibilityDuration = 0.5f;

    [Header("피격 시 카메라 흔들림")] public float shakeDuration = 0.15f; public float shakeMagnitude = 0.25f;

    [Header("피격 시 플레이어 깜빡임")]
    [Tooltip("플레이어 SpriteRenderer")] public SpriteRenderer spriteRenderer;
    [Tooltip("깜빡임 반복 횟수")] public int flashLoops = 4;
    [Tooltip("깜빡임 알파 값")] [Range(0f,1f)] public float flashAlpha = 0.2f;

    private int _currentHealth;
    private float _invincibleTimer;
    private PlayerController _pc;

    public int CurrentHealth => _currentHealth;

    public System.Action<int, int> OnHealthChanged; // current, max
    public System.Action OnPlayerDied;

    private void Awake()
    {
        _currentHealth = maxHealth;
        _pc = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (_invincibleTimer > 0f)
            _invincibleTimer -= Time.deltaTime;
    }

    public void TakeDamage(int dmg)
    {
        if (_pc != null && _pc.IsDashing) return;
        if (_invincibleTimer > 0f) return;

        _currentHealth -= dmg;
        if (_currentHealth < 0) _currentHealth = 0;
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);

        _invincibleTimer = invincibilityDuration;

        CameraShake.Instance?.Shake(shakeDuration, shakeMagnitude);

        // DOTween 깜빡임
        if (spriteRenderer != null)
        {
            spriteRenderer.DOKill();
            Color c = spriteRenderer.color;
            spriteRenderer.DOFade(flashAlpha, 0.08f).SetLoops(flashLoops * 2, LoopType.Yoyo).OnComplete(() => spriteRenderer.color = c);
        }

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