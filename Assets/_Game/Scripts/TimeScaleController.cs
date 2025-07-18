using System.Collections;
using UnityEngine;

/// <summary>
/// 일시적인 슬로우 모션 효과를 중앙에서 관리하는 싱글턴.
/// 여러 곳에서 동시에 슬로우를 요청해도 가장 강한(작은) 배수와 가장 긴 지속 시간을 유지하여
/// 외부 시스템(대화, 일시 정지 등)의 Time.timeScale 변경과 충돌을 최소화합니다.
/// </summary>
public class TimeScaleController : MonoBehaviour
{
    public static TimeScaleController Instance { get; private set; }

    private float _requestedScale = 1f;
    private float _endRealtime = 0f;
    private Coroutine _slowRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 슬로우 모션을 요청합니다.
    /// 같은 프레임에 여러 요청이 들어오면 가장 작은 배수와 가장 긴 지속 시간을 사용합니다.
    /// </summary>
    /// <param name="targetScale">목표 Time.timeScale(0~1)</param>
    /// <param name="duration">지속 시간(실시간 초)</param>
    public void RequestSlow(float targetScale, float duration)
    {
        targetScale = Mathf.Clamp(targetScale, 0.01f, 1f);
        float endTime = Time.realtimeSinceStartup + duration;

        if (_slowRoutine == null)
        {
            _requestedScale = targetScale;
            _endRealtime = endTime;
            _slowRoutine = StartCoroutine(SlowRoutine());
        }
        else
        {
            // 기존 루틴이 실행 중: 값 갱신
            _requestedScale = Mathf.Min(_requestedScale, targetScale);
            _endRealtime = Mathf.Max(_endRealtime, endTime);
        }
    }

    private IEnumerator SlowRoutine()
    {
        // 고정 델타타임 백업
        float originalFixedDelta = Time.fixedDeltaTime;
        while (Time.realtimeSinceStartup < _endRealtime)
        {
            // 외부 시스템이 Time.timeScale = 0 으로 변경(대화/일시정지)했다면 우선순위를 양보
            if (Mathf.Approximately(Time.timeScale, 0f))
            {
                yield return null;
                continue;
            }
            Time.timeScale = _requestedScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return null;
        }

        // 종료 시, 외부에서 다른 값으로 바꿔놨을 수 있으므로 안전 체크
        if (Mathf.Approximately(Time.timeScale, _requestedScale))
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = originalFixedDelta;
        }
        _slowRoutine = null;
    }
} 