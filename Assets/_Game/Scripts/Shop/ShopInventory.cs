using UnityEngine;

/// <summary>
/// 상점에서 판매할 아이템 리스트. 층/스토리 챕터마다 다른 인벤토리를 사용할 수 있다.
/// </summary>
[CreateAssetMenu(fileName = "ShopInventory", menuName = "Run and Gun/Legacy/Shop Inventory", order = 1)]
public class ShopInventory : ScriptableObject
{
    public ShopItemData[] items;
} 