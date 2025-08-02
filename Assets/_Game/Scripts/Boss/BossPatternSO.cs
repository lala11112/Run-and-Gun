using UnityEngine;
using System.Collections;

/// <summary>
/// 보스 패턴의 데이터와 실행 로직을 정의하는 ScriptableObject입니다.
/// </summary>
[CreateAssetMenu(fileName = "BossPattern_New", menuName = "Run and Gun/Boss Pattern", order = 2)]
public class BossPatternSO : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("패턴 이름")]
    public string patternName;
    
    [Tooltip("패턴 설명")]
    [TextArea]
    public string description;

    [Header("실행 설정")]
    [Tooltip("패턴 실행 시간 (초)")]
    public float duration = 3f;
    
    [Tooltip("패턴 실행 후 대기 시간 (초)")]
    public float cooldown = 1f;
    
    [Tooltip("패턴 우선순위 (높을수록 우선 실행)")]
    public int priority = 0;

    [Header("실행 로직")]
    [Tooltip("실제 패턴 로직을 담고 있는 프리팹 (IBossPatternExecutor 구현 필요)")]
    public GameObject patternExecutorPrefab;

    /// <summary>
    /// 패턴을 실행합니다.
    /// </summary>
    public IEnumerator ExecutePattern(BossController bossController)
    {
        if (patternExecutorPrefab == null)
        {
            Debug.LogWarning($"[BossPatternSO] {patternName}의 patternExecutorPrefab이 설정되지 않았습니다.");
            yield break;
        }

        // 패턴 실행기 생성
        var executorObj = Instantiate(patternExecutorPrefab, bossController.transform);
        var executor = executorObj.GetComponent<IBossPatternExecutor>();
        
        if (executor == null)
        {
            Debug.LogError($"[BossPatternSO] {patternName}의 실행기에 IBossPatternExecutor가 없습니다.");
            Destroy(executorObj);
            yield break;
        }

        // 패턴 실행
        yield return executor.Execute(this, bossController);
        
        // 정리
        if (executorObj != null)
        {
            Destroy(executorObj);
        }
        
        // 쿨다운 대기
        if (cooldown > 0)
        {
            yield return new WaitForSeconds(cooldown);
        }
    }
}

/// <summary>
/// 보스 패턴 실행기가 구현해야 하는 인터페이스입니다.
/// </summary>
public interface IBossPatternExecutor
{
    /// <summary>
    /// 패턴을 실행합니다.
    /// </summary>
    /// <param name="patternData">패턴 데이터</param>
    /// <param name="bossController">보스 컨트롤러</param>
    IEnumerator Execute(BossPatternSO patternData, BossController bossController);
}