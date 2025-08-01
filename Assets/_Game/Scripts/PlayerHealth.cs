using System.Collections;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 플레이어의 생명(체력, 피격, 사망)과 관련된 모든 것을 전담하는 컴포넌트입니다.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerHealth : LivingEntity
{
    [Header("데이터")]
    [Tooltip("플레이어의 기본 능력치를 정의하는 ScriptableObject 에셋입니다.")]
    public PlayerStatsSO playerStats;

    [Header("피격 효과")]
    [Tooltip("피격 후 무적 시간(초)")] public float invincibilityDuration = 0.5f;
    [Tooltip("피격 시 사용할 Shake 프리셋 이름")] public string hitShakePreset = "PlayerHit";

    [Header("피격 시 플레이어 깜빡임")]
    [Tooltip("플레이어 SpriteRenderer")] public SpriteRenderer spriteRenderer;
    [Tooltip("깜빡임 반복 횟수")] public int flashLoops = 4;
    [Tooltip("깜빡임 알파 값")] [Range(0f, 1f)] public float flashAlpha = 0.2f;

    [Header("Hit Stop 설정")]
    [Tooltip("피격 시 적용될 Time.timeScale 배수 (0~1)")] [Range(0.05f, 1f)] public float hitStopScale = 0.25f;
    [Tooltip("슬로우 모션 지속 시간(실시간 초)")] public float hitStopDuration = 0.1f;

    // 무적 시간 관리용 타이머
    private float _invincibleTimer;

    private PlayerController _pc;

    // 플레이어가 사망했을 때 호출되는 이벤트 (UI, 게임 오버 처리 등에서 구독)
    public System.Action OnPlayerDied;

    protected override void Awake()
    {
        // PlayerStatsSO 데이터로 LivingEntity의 스탯을 먼저 설정합니다.
        if (playerStats != null)
        {
            maxHealth = (int)playerStats.maxHealth;
            // defense 변수가 LivingEntity에 있다면 defense도 초기화합니다.
            // defense = playerStats.defense; 
        }
        else
        {
            Debug.LogError("[PlayerHealth] PlayerStatsSO 에셋이 연결되지 않았습니다!", this);
        }

        // 부모의 Awake()를 호출하여 스탯을 기반으로 현재 체력을 초기화합니다.
        base.Awake();
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

        // [리팩토링 대상] S랭크 무적 같은 로직은 Style 시스템으로 이전되어야 합니다.
        // 현재는 기존 기능을 유지하기 위해 남겨두지만, 최종적으로는 제거됩니다.
        // if (StyleManager.Instance != null && StyleManager.Instance.CurrentRank == StyleRank.S)
        // {
        //     return;
        // }

        _invincibleTimer = invincibilityDuration;

        CameraManager.Instance?.ShakeWithPreset(hitShakePreset);

        // Hit Stop (TimeScaleController 활용)
        if (TimeScaleController.Instance != null)
        {
            TimeScaleController.Instance.RequestSlow(hitStopScale, hitStopDuration);
        }
        else
        {
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
        GameEvents.RaisePlayerDied();
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
