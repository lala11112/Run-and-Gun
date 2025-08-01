using System.Collections;
using UnityEngine;

/// <summary>
/// ResultState – 결과 화면 씬을 로드하고 데이터를 표시합니다.
/// </summary>
public class ResultState : IState
{
    private readonly StateMachine _sm;
    private readonly GameManager _gm;

    public ResultState(StateMachine sm, GameManager gm)
    {
        _sm = sm;
        _gm = gm;
    }

    public void Enter()
    {
        Debug.Log($"[ResultState] Enter");
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