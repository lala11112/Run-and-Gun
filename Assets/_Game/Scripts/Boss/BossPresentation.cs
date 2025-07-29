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

    [Header("카메라 흔들림 프리셋")] 
    [Tooltip("페이즈 전환 시 사용할 Shake 프리셋 이름")]
    public string phaseShakePreset = "Boss_PhaseChange";
    [Tooltip("사망 시 사용할 Shake 프리셋 이름")]
    public string deathShakePreset = "Boss_Death";

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

        CameraManager.Instance?.ShakeWithPreset(phaseShakePreset);
    }

    private void PlayDeathEffect()
    {
        if (!string.IsNullOrEmpty(deathSfx))
            DarkTonic.MasterAudio.MasterAudio.PlaySound3DAtTransform(deathSfx, transform);

        CameraManager.Instance?.ShakeWithPreset(deathShakePreset);

        // 간단한 scale-out 연출
        transform.DOScale(0f, 1f).SetEase(Ease.InBack).OnComplete(()=>Destroy(gameObject));
    }
} 