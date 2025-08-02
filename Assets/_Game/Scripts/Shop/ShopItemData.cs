using UnityEngine;

/// <summary>
/// 상점에서 판매할 아이템/업그레이드 데이터.
/// </summary>
[CreateAssetMenu(fileName = "ShopItem", menuName = "Run and Gun/Legacy/Shop Item", order = 0)]
public class ShopItemData : ScriptableObject
{
    public string itemId = ""; // 고유 키, SaveData.purchasedUpgrades 에 저장
    public string displayName = "Item";
    [TextArea] public string description;
    public Sprite icon;
    public int price = 10;
    [Tooltip("Skill 업그레이드 등 효과 키")] public string upgradeKey;
} 