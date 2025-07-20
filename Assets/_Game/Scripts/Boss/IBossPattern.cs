using System.Collections;
using UnityEngine;

/// <summary>
/// 모든 보스 패턴이 구현해야 하는 기본 인터페이스.
/// 각 패턴은 코루틴으로 실행되어야 하며, 플레이 중 Cancel 조건에 따라 중단될 수 있습니다.
/// </summary>
public interface IBossPattern
{
    /// <summary>
    /// 패턴 실행 코루틴. 호출 측에서 StartCoroutine으로 실행합니다.
    /// </summary>
    IEnumerator ExecutePattern();
} 