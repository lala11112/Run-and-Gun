using UnityEngine;

/// <summary>
/// 적 기본 능력치 ScriptableObject.
/// 프리팹에 지정하여 밸런스 수치를 중앙에서 관리할 수 있다.
/// </summary>
[CreateAssetMenu(fileName = "EnemyStatData", menuName = "Game/Enemy Stat Data")]
public class EnemyStatData : ScriptableObject
{
    [Header("기본 능력치")]
    [Tooltip("최대 체력")] public int maxHealth = 5;
    [Tooltip("이동 속도 (단위/초)")] public float moveSpeed = 2f;
    [Tooltip("플레이어와 유지할 최소 거리")] public float keepDistance = 6f;

    [Header("공격력 및 AI")] [Tooltip("기본 공격력 (경우에 따라 미사용)")] public int damage = 1;

    [Header("골드 드랍")]
    [Tooltip("드랍 골드 최소값")] public int goldMin = 1;
    [Tooltip("드랍 골드 최대값")] public int goldMax = 3;
} 