using System.Collections;
using UnityEngine;

/// <summary>
/// ResultState – 결과 화면 씬을 로드하고 데이터를 표시합니다.
/// </summary>
public class ResultState : IState
{
    private readonly StateMachine _sm;
    private readonly GameManager _gm;
    private readonly RunResultData _resultData;

    public ResultState(StateMachine sm, GameManager gm, RunResultData resultData)
    {
        _sm = sm;
        _gm = gm;
        _resultData = resultData;
    }

    public void Enter()
    {
        Debug.Log($"[ResultState] Enter – Victory: {_resultData.wasVictory}, Time: {_resultData.timePlayed:F2}s, Gold: {_resultData.goldEarned}");
        _gm.StartCoroutine(LoadResult());
    }

    public void Exit()
    {
        Debug.Log("[ResultState] Exit");
        SaveService.Save();
    }

    public void Tick() { }

    private IEnumerator LoadResult()
    {
        yield return _gm.StartCoroutine(SceneLoader.LoadSceneAsync(_gm.resultSceneName));
        // TODO: 결과 데이터 표시(UIManager 호출 등)
        // 예: ResultPanel.Show(_resultData);
    }
} 