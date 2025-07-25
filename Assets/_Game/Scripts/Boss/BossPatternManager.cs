using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 페이즈별 공격 패턴을 순차적으로 실행하는 매니저.
/// 실제 패턴은 IBossPattern 구현 컴포넌트로 작성.
/// </summary>
[RequireComponent(typeof(BossHealth))]
public class BossPatternManager : MonoBehaviour
{
    [System.Serializable]
    public class PhasePatternList
    {
        [Tooltip("이 페이즈에서 사용할 패턴 컴포넌트들 (순서대로 실행됨)")]
        public List<MonoBehaviour> patterns = new(); // IBossPattern 캐스팅 예정
    }

    [Header("페이즈별 패턴 리스트")]
    public PhasePatternList phase1Patterns;
    public PhasePatternList phase2Patterns;
    public PhasePatternList phase3Patterns;

    private BossHealth _health;
    private Coroutine _runRoutine;

    private void Awake()
    {
        _health = GetComponent<BossHealth>();
        _health.OnPhaseChanged += HandlePhaseChanged;
        StartPhase(_health.CurrentPhase);
    }

    private void HandlePhaseChanged(int phase)
    {
        if (_runRoutine != null) StopCoroutine(_runRoutine);
        StartPhase(phase);
    }

    private void StartPhase(int phase)
    {
        List<MonoBehaviour> list = phase switch
        {
            1 => phase1Patterns.patterns,
            2 => phase2Patterns.patterns,
            3 => phase3Patterns.patterns,
            _ => phase1Patterns.patterns
        };
        _runRoutine = StartCoroutine(RunPatternLoop(list));
    }

    private IEnumerator RunPatternLoop(List<MonoBehaviour> patternList)
    {
        int idx = 0;
        while (true)
        {
            if (patternList.Count == 0) { yield return null; continue; }
            var mb = patternList[idx % patternList.Count];
            if (mb != null && mb is IBossPattern ip)
            {
                yield return StartCoroutine(ip.ExecutePattern());
            }
            idx++;
        }
    }
} 