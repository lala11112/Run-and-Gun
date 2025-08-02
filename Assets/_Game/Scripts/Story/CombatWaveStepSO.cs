using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 개별 적 스폰에 대한 데이터를 정의합니다.
/// </summary>
[System.Serializable]
public struct EnemySpawnData
{
    [Tooltip("스폰할 적의 프리팹")]
    public GameObject enemyPrefab;
    [Tooltip("스폰할 위치 (맵 프리팹 내 SpawnPoint 오브젝트의 이름과 일치해야 함)")]
    public string spawnPointName;
    [Tooltip("이 위치에 몇 마리를 스폰할지")]
    public int count;
}

/// <summary>
/// '전투 웨이브' 스텝의 데이터를 정의하는 ScriptableObject입니다.
/// </summary>
[CreateAssetMenu(fileName = "CombatWaveStep", menuName = "Run and Gun/Story/Steps/Combat Wave")]
public class CombatWaveStepSO : StoryStepSO
{
    [Header("전투 웨이브 설정")]
    [Tooltip("이번 웨이브에서 스폰할 적들의 목록")]
    public List<EnemySpawnData> waveSpawns = new List<EnemySpawnData>();

    public override IStoryStepState CreateState(StoryPlayer storyPlayer)
    {
        return new CombatWaveState(this, storyPlayer);
    }
}

/// <summary>
/// CombatWaveStepSO의 데이터를 받아 실제 로직을 처리하는 상태 클래스입니다.
/// </summary>
public class CombatWaveState : IStoryStepState
{
    private readonly CombatWaveStepSO _data;
    private readonly StoryPlayer _storyPlayer;
    private int _remainingEnemies;
    private Dictionary<string, Transform> _spawnPoints = new Dictionary<string, Transform>();

    public CombatWaveState(CombatWaveStepSO data, StoryPlayer storyPlayer)
    {
        _data = data;
        _storyPlayer = storyPlayer;
    }

    public void Enter()
    {
        // 1. 현재 맵에서 스폰 포인트들을 찾아서 캐싱
        CacheSpawnPoints();
        
        // 2. 적 스폰 및 남은 적 카운트
        SpawnEnemies();
        
        if (_remainingEnemies == 0)
        {
            // 스폰할 적이 없으면 바로 다음 스텝으로
            Debug.LogWarning("[CombatWaveState] 스폰할 적이 설정되지 않았습니다. 스텝을 건너뜁니다.");
            _storyPlayer.AdvanceToNextStep();
            return;
        }

        // 3. 적 사망 이벤트 구독
        GameEvents.EnemyDied += OnEnemyDied;
    }

    private void CacheSpawnPoints()
    {
        _spawnPoints.Clear();
        if (StoryPlayerContext.CurrentMap == null)
        {
            Debug.LogError("[CombatWaveState] 스폰 포인트를 찾을 수 없습니다. 현재 맵이 없습니다!");
            return;
        }

        // SpawnPoint 태그를 가진 모든 자식 오브젝트를 찾음
        foreach (Transform child in StoryPlayerContext.CurrentMap.transform)
        {
            if (child.CompareTag("SpawnPoint"))
            {
                _spawnPoints[child.name] = child;
            }
        }
    }

    private void SpawnEnemies()
    {
        _remainingEnemies = 0;
        foreach (var spawnData in _data.waveSpawns)
        {
            if (spawnData.enemyPrefab == null) continue;

            if (_spawnPoints.TryGetValue(spawnData.spawnPointName, out var spawnPoint))
            {
                for (int i = 0; i < spawnData.count; i++)
                {
                    // TODO: 오브젝트 풀링이 있다면 교체 (PoolManager.Spawn(...))
                    Object.Instantiate(spawnData.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                    _remainingEnemies++;
                }
            }
            else
            {
                Debug.LogWarning($"[CombatWaveState] 스폰 포인트 '{spawnData.spawnPointName}'를 찾을 수 없습니다.");
            }
        }
    }

    private void OnEnemyDied(bool isBoss)
    {
        // 이 스텝은 일반 적만 카운트합니다.
        if (isBoss) return;

        _remainingEnemies--;
        if (_remainingEnemies <= 0)
        {
            Debug.Log("[CombatWaveState] 모든 적이 처치되었습니다. 다음 스텝으로 진행합니다.");
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
