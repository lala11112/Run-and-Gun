using System;
using UnityEngine;

/// <summary>
/// 로그라이크 모드 – 라운드/층 반복 구조.
/// 현재는 더미 구현으로 로그만 출력합니다.
/// </summary>
public class RoguelikeMode : IGameMode
{
    public event Action<bool> OnRunEnded;

    [Tooltip("층별 설정 리스트(순서) – 인스펙터에서 할당")] public FloorConfig[] floorConfigs;
    private int _currentFloor = 0;
    private LevelGenerator _generator;

    public void Initialize()
    {
        Debug.Log("[RoguelikeMode] Initialize");
        _generator = new GameObject("LevelGenerator").AddComponent<LevelGenerator>();
        StartRun();
    }

    public void StartRun()
    {
        _currentFloor++;
        Debug.Log($"[RoguelikeMode] StartRun – Floor {_currentFloor}");
        FloorConfig cfg = floorConfigs.Length>0 ? floorConfigs[Mathf.Min(_currentFloor-1, floorConfigs.Length-1)] : null;
        if (cfg == null)
        {
            Debug.LogError("[RoguelikeMode] FloorConfig 가 없습니다.");
            return;
        }
        _generator.StartCoroutine(_generator.Generate(cfg,_currentFloor,(enemies,boss)=>
        {
            // 스폰 완료 후 적/보스 모니터링
            GameEvents.EnemyDied += HandleEnemyDied;
            _remaining   = enemies.Count;
            _bossAlive = boss != null;
        }));
    }

    private int _remaining;
    private bool _bossAlive;
    private void HandleEnemyDied(bool isBoss)
    {
        if (isBoss) _bossAlive=false; else _remaining = Mathf.Max(0,_remaining-1);
        if(!_bossAlive && _remaining==0)
        {
            GameEvents.EnemyDied -= HandleEnemyDied;
            // 층 완료 – 상점이 있으면 상점 열고 다음 층, 없으면 계속
            Debug.Log("[RoguelikeMode] Floor clear");
            StartRun();
        }
    }

    public void EndRun(bool victory)
    {
        Debug.Log($"[RoguelikeMode] EndRun – Victory:{victory}");
        OnRunEnded?.Invoke(victory);
    }

    public void Cleanup()
    {
        Debug.Log("[RoguelikeMode] Cleanup");
        if(_generator!=null) GameObject.Destroy(_generator.gameObject);
    }
} 