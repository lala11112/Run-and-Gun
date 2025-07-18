using UnityEngine;

/// <summary>
/// 카메라 흔들림 관리. CameraFollow가 위치를 갱신한 뒤 실행되도록 ExecutionOrder 지정.
/// </summary>
[DefaultExecutionOrder(100)]
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Tooltip("감쇠 속도")] public float dampingSpeed = 2f;

    private float _shakeDuration;
    private float _shakeMagnitude;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// 흔들림 요청.
    /// </summary>
    public void Shake(float duration, float magnitude)
    {
        _shakeDuration = Mathf.Max(_shakeDuration, duration);
        _shakeMagnitude = Mathf.Max(_shakeMagnitude, magnitude);
    }

    private void LateUpdate()
    {
        // CameraFollow가 계산한 위치를 기준으로 오프셋 적용
        Vector3 basePos = transform.position;

        if (_shakeDuration > 0f)
        {
            Vector2 rand = Random.insideUnitCircle * _shakeMagnitude;
            transform.position = basePos + new Vector3(rand.x, rand.y, 0f);

            _shakeDuration -= Time.deltaTime * dampingSpeed;
        }
    }
} 