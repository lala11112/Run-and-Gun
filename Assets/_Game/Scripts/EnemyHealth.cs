using System.Collections;
using UnityEngine;
using DarkTonic.MasterAudio;

/// <summary>
/// 적 전용 체력 컴포넌트.
/// 사망 사운드, 카메라 흔들림, 슬로우 모션 등 연출을 담당합니다.
/// Enemy.cs 는 이동/AI 담당, 체력/사망은 본 컴포넌트가 담당합니다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyHealth : LivingEntity
{
    [Header("사망 연출")]
    [Tooltip("사망 사운드의 Sound Group 이름")] public string dieSfx = "EnemyDie";
    [Tooltip("사망 시 카메라 흔들림 지속 시간(초)")] public float dieShakeDuration = 0.25f;
    [Tooltip("사망 시 카메라 흔들림 세기")] public float dieShakeMagnitude = 0.3f;
    [Tooltip("사망 시 적용할 타임스케일 배수 (1 = 정상 속도)")] [Range(0.05f,1f)] public float dieSlowScale = 0.2f;
    [Tooltip("슬로우 모션 지속 시간(실시간 초)")] public float dieSlowDuration = 0.15f;

    protected override void Die()
    {
        StartCoroutine(DieSequence());
    }

    private IEnumerator DieSequence()
    {
        // 1) 사운드 재생
        if (!string.IsNullOrEmpty(dieSfx))
        {
            MasterAudio.PlaySound3DAtTransformAndForget(dieSfx, transform);
        }

        // 2) 카메라 흔들림
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(dieShakeDuration, dieShakeMagnitude);
        }

        // 3) 슬로우 모션
        if (TimeScaleController.Instance != null)
        {
            TimeScaleController.Instance.RequestSlow(dieSlowScale, dieSlowDuration);
        }
        else
        {
            float originalScale = Time.timeScale;
            float originalFixed = Time.fixedDeltaTime;
            Time.timeScale = dieSlowScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return new WaitForSecondsRealtime(dieSlowDuration);
            if (Mathf.Approximately(Time.timeScale, dieSlowScale))
            {
                Time.timeScale = originalScale;
                Time.fixedDeltaTime = originalFixed;
            }
        }

        // 4) 실제 파괴
        Destroy(gameObject);
    }
} 