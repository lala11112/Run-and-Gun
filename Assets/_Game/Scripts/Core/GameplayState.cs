using System.Collections;
using UnityEngine;

/// <summary>
/// GameplayState – 실제 게임플레이 씬을 로드하고 게임 모드 실행을 시작합니다.
/// </summary>
public class GameplayState : IState
{
    private readonly StateMachine _sm;
    private readonly GameManager _gm;
    private int  _remainingEnemies;
    private bool _bossAlive;
    private bool _bossSpawned;

    public GameplayState(StateMachine sm, GameManager gm)
    {
        _sm = sm;
        _gm = gm;
    }

    public void Enter()
    {
        Debug.Log("[GameplayState] Enter – Gameplay 씬 로딩");
        _gm.StartCoroutine(LoadAndStart());
        GameEvents.EnemyDied += OnEnemyDied; // 구독
        BossHealth.OnBossSpawned += OnBossSpawned;
    }

    public void Exit()
    {
        Debug.Log("[GameplayState] Exit – Gameplay 정리");
        // HUD 숨김, 타임스케일 리셋 등 필요 작업
        GameEvents.EnemyDied -= OnEnemyDied;
        BossHealth.OnBossSpawned -= OnBossSpawned;
    }

    public void Tick()
    {
        // 런타임 게임플레이 갱신이 필요하면 여기에 처리
    }

    private IEnumerator LoadAndStart()
    {
        yield return _gm.StartCoroutine(SceneLoader.LoadSceneAsync(_gm.gameplaySceneName));
        // TODO: GameMode 실행 – 현재는 로그만 출력
        Debug.Log("[GameplayState] Gameplay 시작!");

        // 초기 생존 적/보스 카운트 저장
        _remainingEnemies = Object.FindObjectsOfType<EnemyHealth>().Length;
        _bossAlive        = Object.FindObjectsOfType<BossHealth>().Length > 0;
        _bossSpawned      = _bossAlive; // 초기 씬에 보스 있으면 스폰 플래그 true
    }

    private void OnEnemyDied(bool isBoss)
    {
        // 모든 적과 보스가 소멸했는지 확인
        if (isBoss)
        {
            _bossAlive = false;
        }
        else
        {
            _remainingEnemies = Mathf.Max(0, _remainingEnemies - 1);
        }

        // 승리 조건: 보스가 존재했던 러닝이고, 현재 보스와 적이 모두 없다.
        if (_bossSpawned && !_bossAlive && _remainingEnemies == 0)
        {
            _gm.LastRunVictory = true;
            _sm.ChangeState(new ResultState(_sm, _gm, true));
        }
    }

    private void OnBossSpawned(BossHealth bh)
    {
        _bossAlive   = true;
        _bossSpawned = true;
    }
} 