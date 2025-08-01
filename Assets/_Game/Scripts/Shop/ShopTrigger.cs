#if false // 전체 스크립트를 비활성화합니다.

using UnityEngine;

/// <summary>
/// 플레이어가 트리거에 진입하면 지정된 ShopInventory를 열어 주는 간단한 컴포넌트.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ShopTrigger : MonoBehaviour
{
    [Tooltip("열 ShopInventory ScriptableObject")] public ShopInventory inventory;
    [Tooltip("한 번만 열리도록 할지 여부")] public bool oneTime = true;

    private bool _opened;

    private void Reset()
    {
        // 2D Collider를 트리거로 설정
        if (TryGetComponent(out Collider2D col))
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (oneTime && _opened) return;
        if (inventory == null)
        {
            Debug.LogWarning("[ShopTrigger] Inventory가 비어 있습니다.");
            return;
        }
        //ShopPanel.OpenShop(inventory);
        _opened = true;
    }
}
#endif // 전체 스크립트 비활성화 종료