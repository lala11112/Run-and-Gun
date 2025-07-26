using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// FloorConfig 를 바탕으로 맵을 생성, NavMesh 점검, 적/보스 스폰까지 처리하는 유틸리티.
/// Level 생성 완료 후 콜백을 통해 알립니다.
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    public IEnumerator Generate(FloorConfig cfg, int floorIndex, System.Action<List<SimpleEnemy>,BossHealth> onFinished)
    {
        // 1) 기존 맵 정리
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.CompareTag("GeneratedMap")) Destroy(root);
        }

        // 2) 맵 프리팹 인스턴스
        var mapPrefab = cfg.mapPrefabs[Random.Range(0, cfg.mapPrefabs.Length)];
        var map = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
        map.tag = "GeneratedMap";

        // 3) NavMeshSurface2D 가 있다면 Bake 생략 (프리팹 포함 가정)

        // 4) 적/보스 스폰
        var enemies = new List<SimpleEnemy>();
        if (cfg.bossPrefab != null && floorIndex % 3 == 0) // 예: 3의 배수층에 보스
        {
            var bh = Instantiate(cfg.bossPrefab, Vector3.up * 4f, Quaternion.identity).GetComponent<BossHealth>();
            onFinished?.Invoke(enemies, bh);
            yield break;
        }
        int spawnMin = cfg.spawnCountRange.x + cfg.spawnIncrementPerFloor * (floorIndex-1);
        int spawnMax = cfg.spawnCountRange.y + cfg.spawnIncrementPerFloor * (floorIndex-1);
        int count = Random.Range(spawnMin, spawnMax+1);
        for(int i=0;i<count;i++)
        {
            var epref = cfg.enemyPrefabs[Random.Range(0,cfg.enemyPrefabs.Length)];
            Vector2 offset2D = Random.insideUnitCircle * 5f;
            var pos = new Vector3(offset2D.x, offset2D.y, 0f); // TODO: NavMesh 샘플
            var e = Instantiate(epref,pos,Quaternion.identity).GetComponent<SimpleEnemy>();
            enemies.Add(e);
            yield return null; // 분산 생성
        }
        onFinished?.Invoke(enemies,null);
    }
} 