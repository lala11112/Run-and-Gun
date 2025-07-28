using System.Collections;
using UnityEngine;

/// <summary>
/// GameplayState – 실제 게임플레이 씬을 로드하고 게임 모드 실행을 시작합니다.
/// </summary>
public class GameplayState : IState
{
    private readonly StateMachine _sm;
    private readonly GameManager _gm;
    private readonly IGameMode _activeMode;
    private int  _remainingEnemies;
    private bool _bossAlive;
    private bool _bossSpawned;
    private float _startTime;
    private int _startGold;
    private bool _sceneLoaded = false;

    public GameplayState(StateMachine sm, GameManager gm, IGameMode activeMode)
    {
        _sm = sm;
        _gm = gm;
        _activeMode = activeMode;
    }

    public void Enter()
    {
        Debug.Log("[GameplayState] Enter – Gameplay 씬 로딩");
        if (!_sceneLoaded)
        {
            _gm.StartCoroutine(LoadAndStart());
        }
        else
        {
            // 이미 씬이 로드된 상태에서 다시 진입하는 경우 (예: 일시정지 해제)
            // 추가적인 로직 없이 바로 게임플레이를 재개합니다.
        }
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
        _sceneLoaded = true; // 씬 로드가 완료되었음을 표시
        _activeMode?.Initialize(); // 모드 초기화
        Debug.Log("[GameplayState] Gameplay 시작!");

        // 초기 생존 적/보스 카운트 저장
        _remainingEnemies = Object.FindObjectsOfType<EnemyHealth>().Length;
        _bossAlive        = Object.FindObjectsOfType<BossHealth>().Length > 0;
        _bossSpawned      = _bossAlive; // 초기 씬에 보스 있으면 스폰 플래그 true

        // 플레이 시간 및 골드 획득량 측정을 위한 초기값 저장
        _startTime = Time.time;
        _startGold = SaveService.Data.gold;
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
            EndRun(true);
        }
    }

    private void OnBossSpawned(BossHealth bh)
    {
        _bossAlive   = true;
        _bossSpawned = true;
    }

    private void EndRun(bool victory)
    {
        var result = new RunResultData
        {
            wasVictory = victory,
            timePlayed = Time.time - _startTime,
            goldEarned = SaveService.Data.gold - _startGold
        };
        _sm.ChangeState(new ResultState(_sm, _gm, result));
    }

    // GameManager에서 PlayerDied 이벤트 수신 시 이 메서드를 직접 호출하도록 변경할 것입니다.
    public void HandlePlayerDied()
    {
        EndRun(false);
    }

    public void PauseGame()
    {
        if (_activeMode == null) return;

        // 활성 모드로부터 올바른 PauseMenu 프리팹을 가져옵니다.
        var pauseMenuPrefab = _activeMode.PauseMenuPrefab;
        // 활성 모드로부터 컨텍스트 데이터를 가져옵니다.
        var context = _activeMode.GetPauseMenuContext();
        _sm.ChangeState(new PausedState(_sm, _gm, this, pauseMenuPrefab, context));
    }
} 