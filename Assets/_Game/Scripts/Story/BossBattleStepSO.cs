using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// '보스전' 스텝의 데이터를 정의하는 ScriptableObject입니다.
/// </summary>
[CreateAssetMenu(fileName = "BossBattleStep", menuName = "Run and Gun/Story/Steps/Boss Battle")]
public class BossBattleStepSO : StoryStepSO
{
    [Header("보스전 설정")]
    [Tooltip("스폰할 보스의 프리팹")]
    public GameObject bossPrefab;
    [Tooltip("보스를 스폰할 위치 (맵 프리팹 내 SpawnPoint 오브젝트의 이름과 일치해야 함)")]
    public string spawnPointName;

    public override IStoryStepState CreateState(StoryPlayer storyPlayer)
    {
        return new BossBattleState(this, storyPlayer);
    }
}

/// <summary>
/// BossBattleStepSO의 데이터를 받아 실제 로직을 처리하는 상태 클래스입니다.
/// </summary>
public class BossBattleState : IStoryStepState
{
    private readonly BossBattleStepSO _data;
    private readonly StoryPlayer _storyPlayer;
    private bool _isBossDefeated = false;

    public BossBattleState(BossBattleStepSO data, StoryPlayer storyPlayer)
    {
        _data = data;
        _storyPlayer = storyPlayer;
    }

    public void Enter()
    {
        if (_data.bossPrefab == null)
        {
            Debug.LogWarning("[BossBattleState] 보스 프리팹이 설정되지 않았습니다. 스텝을 건너뜁니다.");
            _storyPlayer.AdvanceToNextStep();
            return;
        }

        // 1. 스폰 포인트 찾기
        Transform spawnPoint = FindSpawnPoint(_data.spawnPointName);
        if (spawnPoint == null)
        {
            Debug.LogError($"[BossBattleState] 스폰 포인트 '{_data.spawnPointName}'를 찾을 수 없어 보스를 스폰할 수 없습니다.");
            _storyPlayer.AdvanceToNextStep(); // 진행이 막히지 않도록 다음으로 넘김
            return;
        }

        // 2. 보스 스폰
        // TODO: 오브젝트 풀링이 있다면 교체 (PoolManager.Spawn(...))
        Object.Instantiate(_data.bossPrefab, spawnPoint.position, spawnPoint.rotation);
        _isBossDefeated = false;

        // 3. 적(보스) 사망 이벤트 구독
        GameEvents.EnemyDied += OnEnemyDied;
    }

    private Transform FindSpawnPoint(string pointName)
    {
        if (StoryPlayerContext.CurrentMap == null) return null;

        // SpawnPoint 태그를 가진 자식 오브젝트 중 이름이 일치하는 것을 찾음
        foreach (Transform child in StoryPlayerContext.CurrentMap.transform)
        {
            if (child.CompareTag("SpawnPoint") && child.name == pointName)
            {
                return child;
            }
        }
        return null;
    }

    private void OnEnemyDied(bool isBoss)
    {
        // 보스가 죽었을 때만 반응
        if (isBoss)
        {
            // 중복 호출 방지
            if (_isBossDefeated) return;
            _isBossDefeated = true;

            Debug.Log("[BossBattleState] 보스가 처치되었습니다. 다음 스텝으로 진행합니다.");
            _storyPlayer.AdvanceToNextStep();
        }
    }

    public void Tick() { }

    public void Exit()
    {
        // 중요: 이벤트 구독 해제
        GameEvents.EnemyDied -= OnEnemyDied;
    }
}
