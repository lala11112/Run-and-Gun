#if false // 전체 스크립트를 비활성화합니다.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ShopPanel – 주어진 ShopInventory 를 UI로 표시하고 구매 처리.
/// 사전에 itemUIPrefab (Button + Icon + Text) 을 설정해야 함.
/// </summary>
public class ShopPanel : MonoBehaviour
{
    [Tooltip("아이템 슬롯이 배치될 부모 트랜스폼")] public Transform contentRoot;
    [Tooltip("아이템 UI 프리팹 – Button 컴포넌트 필요")] public GameObject itemUIPrefab;
    [Tooltip("Close 버튼")] public Button closeButton;

    private ShopInventory _inventory;

    private void Awake()
    {
        if (closeButton != null) closeButton.onClick.AddListener(() => UIManager.Instance.Pop());
    }

    public void Initialize(ShopInventory inv)
    {
        _inventory = inv;
        foreach (Transform child in contentRoot) Destroy(child.gameObject);
        foreach (var item in _inventory.items)
        {
            var ui = Instantiate(itemUIPrefab, contentRoot);
            SetupItemUI(ui, item);
        }
    }

    private void SetupItemUI(GameObject ui, ShopItemData data)
    {
        if (ui.TryGetComponent(out Button btn))
        {
            btn.onClick.AddListener(() => TryPurchase(data));
        }
        var txt = ui.GetComponentInChildren<TMP_Text>();
        if (txt != null)
        {
            txt.text = $"{data.displayName} \n<color=yellow>{data.price}G</color>";
        }
        var img = ui.GetComponentInChildren<Image>();
        if (img != null && data.icon != null)
        {
            img.sprite = data.icon;
        }
    }

    private void TryPurchase(ShopItemData item)
    {
        if (System.Array.Exists(SaveService.Data.purchasedUpgrades, id => id == item.itemId))
        {
            Debug.Log("[Shop] 이미 구매한 아이템입니다");
            return;
        }
        if (!CurrencyService.Instance.SpendGold(item.price))
        {
            Debug.Log("[Shop] 골드가 부족합니다");
            return;
        }
        // 효과 적용
        //bool applied = SkillManager.Instance != null && SkillManager.Instance.ApplyUpgrade(item.upgradeKey);
        // 구매 기록 저장
        var list = new System.Collections.Generic.List<string>(SaveService.Data.purchasedUpgrades);
        list.Add(item.itemId);
        SaveService.Data.purchasedUpgrades = list.ToArray();
        SaveService.Save();
        GameEvents.RaiseGoldChanged(CurrencyService.Instance.Gold);
        //Debug.Log($"[Shop] {item.displayName} 구매 완료 (적용:{applied})");
    }

    // ----------- Helper -----------
    public static void OpenShop(ShopInventory inv)
    {
        var prefab = Resources.Load<GameObject>("UI/ShopPanel");
        if (prefab != null)
        {
            var panel = Instantiate(prefab);
            var sp = panel.GetComponent<ShopPanel>();
            sp.Initialize(inv);
            UIManager.Instance.Push(panel);
        }
        else
        {
            Debug.LogWarning("[ShopPanel] Resources/UI/ShopPanel 프리팹이 없습니다.");
        }
    }
}
#endif // 전체 스크립트 비활성화 종료