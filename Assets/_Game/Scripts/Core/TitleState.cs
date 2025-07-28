using System.Collections;
using UnityEngine;

/// <summary>
/// TitleState – 타이틀 씬을 로드하고 메뉴 입력을 대기합니다.
/// </summary>
public class TitleState : IState
{
    private readonly StateMachine _sm;
    private readonly GameManager _gm;

    public TitleState(StateMachine sm, GameManager gm)
    {
        _sm = sm;
        _gm = gm;
    }

    public void Enter()
    {
        Debug.Log("[TitleState] Enter – 타이틀 씬 로딩 시작");
        _gm.StartCoroutine(LoadTitle());
    }

    public void Exit()
    {
        Debug.Log("[TitleState] Exit – 타이틀 UI 정리");
    }

    public void Tick()
    {
        // 메뉴 버튼 클릭이 TitleMenuController에서 처리됩니다.
    }

    private IEnumerator LoadTitle()
    {
        // GameManager의 ReturnToTitle에서 이미 Time.timeScale을 1로 복구했지만,
        // 상태 전환 과정에서 확실하게 처리해주는 것이 더 안전합니다.
        Time.timeScale = 1f;
        yield return _gm.StartCoroutine(SceneLoader.LoadSceneAsync(_gm.titleSceneName));
        // 씬이 로드된 후 BGM을 재생하는 등의 추가 작업을 할 수 있습니다.
    }
} 