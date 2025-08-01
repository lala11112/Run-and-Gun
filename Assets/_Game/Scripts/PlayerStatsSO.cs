using UnityEngine;

/// <summary>
/// 플레이어의 핵심 능력치를 정의하는 ScriptableObject입니다.
/// 이 데이터를 교체하여 다양한 유형의 플레이어(전사, 마법사 등)를 쉽게 만들 수 있습니다.
/// </summary>
[CreateAssetMenu(fileName = "PlayerStats_New", menuName = "Run and Gun/Player Stats", order = 0)]
public class PlayerStatsSO : ScriptableObject
{
    [Header("체력 관련")]
    [Tooltip("플레이어의 최대 체력입니다.")]
    public float maxHealth = 100f;

    [Tooltip("플레이어의 기본 방어력입니다.")]
    public float defense = 0f;

    [Header("이동 관련")]
    [Tooltip("플레이어의 기본 이동 속도입니다.")]
    public float moveSpeed = 5f;
    
    // 추후 다른 스탯들을 여기에 추가할 수 있습니다. (예: 공격력, 치명타 확률 등)
}
