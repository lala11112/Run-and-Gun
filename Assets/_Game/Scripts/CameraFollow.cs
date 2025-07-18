using UnityEngine;

/// <summary>
/// 카메라가 타겟(플레이어)을 부드럽게 따라가도록 하는 스크립트
/// 탑다운 시점에서 고정된 오프셋을 유지하며 추적
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("추적 설정")]
    [Tooltip("따라갈 대상 오브젝트 (보통 플레이어)")]
    public Transform target;
    
    [Tooltip("타겟으로부터의 카메라 오프셋 (x, y, z)")]
    public Vector3 offset = new Vector3(0f, 15f, -2f); // 약간의 기울기
    
    [Tooltip("카메라 이동의 부드러움 정도 (낮을수록 빠르게 추적)")]
    public float smoothTime = 0.15f;

    // 내부 변수
    private Vector3 _velocity; // SmoothDamp에서 사용하는 속도 벡터
    private Quaternion _initialRotation;

    private void Awake()
    {
        _initialRotation = transform.rotation;
    }

    /// <summary>
    /// LateUpdate에서 카메라 위치만 업데이트 (회전 고정)
    /// </summary>
    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _velocity, smoothTime);

        // 2D 게임이므로 회전은 고정
        transform.rotation = _initialRotation;
    }
} 