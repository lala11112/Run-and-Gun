using UnityEngine;
using DG.Tweening;

/// <summary>
/// 보스 상태 전환과 사망 시 연출(애니메이션/사운드/카메라)을 담당.
/// 실제 연출은 인스펙터에서 할당한 프리팹·사운드 이름 등을 호출.
/// </summary>
[RequireComponent(typeof(BossHealth))]
public class BossPresentation : MonoBehaviour
{
    [Header("사운드 설정")] [Tooltip("페이즈 변경 시 재생할 사운드")] public string phaseChangeSfx = "BossPhase";
    [Tooltip("사망 시 재생할 사운드")] public string deathSfx = "BossDie";

    [Header("카메라 흔들림 설정")] [Tooltip("페이즈 전환 시 흔들림")]
    public Vector2 phaseShake = new Vector2(0.2f,0.4f);
    [Tooltip("사망 시 흔들림")]
    public Vector2 deathShake = new Vector2(0.3f,0.6f);

    private void Awake()
    {
        var health = GetComponent<BossHealth>();
        health.OnPhaseChanged += p => PlayPhaseChangeEffect();
        health.OnBossDead += PlayDeathEffect;
    }

    private void PlayPhaseChangeEffect()
    {
        if (!string.IsNullOrEmpty(phaseChangeSfx))
            DarkTonic.MasterAudio.MasterAudio.PlaySound3DAtTransform(phaseChangeSfx, transform);

        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(phaseShake.y, phaseShake.x);
    }

    private void PlayDeathEffect()
    {
        if (!string.IsNullOrEmpty(deathSfx))
            DarkTonic.MasterAudio.MasterAudio.PlaySound3DAtTransform(deathSfx, transform);

        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(deathShake.y, deathShake.x);

        // 간단한 scale-out 연출
        transform.DOScale(0f, 1f).SetEase(Ease.InBack).OnComplete(()=>Destroy(gameObject));
    }
} 