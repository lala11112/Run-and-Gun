using System.Collections;
using UnityEngine;

/// <summary>
/// ResultState – 결과 화면 씬을 로드하고 데이터를 표시합니다.
/// </summary>
public class ResultState : IState
{
    private readonly StateMachine _sm;
    private readonly GameManager _gm;
    private readonly bool _victory;

    public ResultState(StateMachine sm, GameManager gm, bool victory = false)
    {
        _sm = sm;
        _gm = gm;
        _victory = victory;
    }

    public void Enter()
    {
        Debug.Log($"[ResultState] Enter – 결과 씬 로딩, Victory={_victory}");
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
        Debug.Log(_victory ? "[ResultState] 승리!" : "[ResultState] 패배...");
    }
} 