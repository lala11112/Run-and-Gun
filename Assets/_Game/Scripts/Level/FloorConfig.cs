using UnityEngine;

/// <summary>
/// 로그라이크 한 층(라운드)에 사용할 맵/적/보스/상점 설정.
/// 층별 난이도 스케일링이나 특수 룰을 손쉽게 조정 가능.
/// </summary>
[CreateAssetMenu(fileName = "FloorConfig", menuName = "Game/Floor Config", order = 0)]
public class FloorConfig : ScriptableObject
{
    [Tooltip("맵 프리팹 리스트 (랜덤 선택)")] public GameObject[] mapPrefabs;
    [Tooltip("일반 적 프리팹 리스트")]  public GameObject[] enemyPrefabs;
    [Tooltip("보스 프리팹 (null이면 보스 없는 층)")] public GameObject bossPrefab;
    [Tooltip("상점 인벤토리 (null이면 상점 없음)")] public ShopInventory shopInventory;

    [Tooltip("적 스폰 수 (min,max)")] public Vector2Int spawnCountRange = new(5,10);
    [Tooltip("적 수 라운드별 증가량")] public int spawnIncrementPerFloor = 0;
} 