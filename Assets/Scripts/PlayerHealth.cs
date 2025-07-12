using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

/// <summary>
/// 플레이어 체력 관리 및 피격 시 처리.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerHealth : LivingEntity
{
    [Tooltip("피격 후 무적 시간(초)")] public float invincibilityDuration = 0.5f;

    [Header("피격 시 카메라 흔들림")] public float shakeDuration = 0.15f; public float shakeMagnitude = 0.25f;

    [Header("피격 시 플레이어 깜빡임")]
    [Tooltip("플레이어 SpriteRenderer")] public SpriteRenderer spriteRenderer;
    [Tooltip("깜빡임 반복 횟수")] public int flashLoops = 4;
    [Tooltip("깜빡임 알파 값")] [Range(0f,1f)] public float flashAlpha = 0.2f;

    [Header("Hit Stop 설정")]
    [Tooltip("피격 시 적용될 Time.timeScale 배수 (0~1)")] [Range(0.05f,1f)] public float hitStopScale = 0.25f;
    [Tooltip("슬로우 모션 지속 시간(실시간 초)")] public float hitStopDuration = 0.1f;

    // 무적 시간 관리용 타이머
    private float _invincibleTimer;

    private PlayerController _pc;

    // 플레이어가 사망했을 때 호출되는 이벤트 (UI, 게임 오버 처리 등에서 구독)
    public System.Action OnPlayerDied;

    protected override void Awake()
    {
        base.Awake(); // LivingEntity 초기화
        _pc = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (_invincibleTimer > 0f)
            _invincibleTimer -= Time.deltaTime;
    }

    public override void TakeDamage(int dmg)
    {
        if (_pc != null && _pc.IsDashing) return;
        if (_invincibleTimer > 0f) return;

        _invincibleTimer = invincibilityDuration;

        CameraShake.Instance?.Shake(shakeDuration, shakeMagnitude);

        // Hit Stop (TimeScaleController 활용)
        if (TimeScaleController.Instance != null)
        {
            TimeScaleController.Instance.RequestSlow(hitStopScale, hitStopDuration);
        }
        else
        {
            // 백업: 컨트롤러가 없을 때 직접 처리
            StartCoroutine(HitStopRoutine(hitStopScale, hitStopDuration));
        }

        // DOTween 깜빡임
        if (spriteRenderer != null)
        {
            spriteRenderer.DOKill();
            Color c = spriteRenderer.color;
            spriteRenderer.DOFade(flashAlpha, 0.08f).SetLoops(flashLoops * 2, LoopType.Yoyo).OnComplete(() => spriteRenderer.color = c);
        }

        // 실제 체력 감소 및 사망 판정은 LivingEntity에 위임
        base.TakeDamage(dmg);
    }

    protected override void Die()
    {
        OnPlayerDied?.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator HitStopRoutine(float scale, float dur)
    {
        scale = Mathf.Clamp(scale, 0.05f, 1f);
        float originalScale = Time.timeScale;
        float originalFixed = Time.fixedDeltaTime;
        Time.timeScale = scale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        yield return new WaitForSecondsRealtime(dur);
        if (Mathf.Approximately(Time.timeScale, scale))
        {
            Time.timeScale = originalScale;
            Time.fixedDeltaTime = originalFixed;
        }
    }
} 