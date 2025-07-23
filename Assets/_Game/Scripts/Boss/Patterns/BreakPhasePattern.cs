using System.Collections;
using UnityEngine;

/// <summary>
/// [Break Phase] – 3단 공격 후 약점 노출.
/// (패턴 실행 후 BossHitbox 또는 별도 WeakPoint 활성화 구현 필요)
/// </summary>
public class BreakPhasePattern : MonoBehaviour, IBossPattern
{
    [Tooltip("전방 탄막 패턴 컴포넌트")] public PrecisionSpreadPattern forwardSpread;
    [Tooltip("회전 탄막 패턴 컴포넌트")] public RotaryRingPattern rotary;
    [Tooltip("돌진 거리")] public float chargeDistance = 5f;
    [Tooltip("돌진 속도")] public float chargeSpeed = 6f;
    [Tooltip("약점 노출 시간")] public float weakDuration = 3f;

    public Transform weakPoint; // 약점 오브젝트(히트박스 비활성 상태)

    public IEnumerator ExecutePattern()
    {
        if (forwardSpread != null) yield return forwardSpread.ExecutePattern();
        if (rotary != null) yield return rotary.ExecutePattern();

        // 돌진 – 간단히 위치 보간
        Vector3 start = transform.position;
        Vector3 target = start + Vector3.down * chargeDistance;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * chargeSpeed / chargeDistance;
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        // 약점 활성
        if (weakPoint != null) weakPoint.gameObject.SetActive(true);
        yield return new WaitForSeconds(weakDuration);
        if (weakPoint != null) weakPoint.gameObject.SetActive(false);
    }
} 