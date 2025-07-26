using UnityEngine;

/// <summary>
/// 골드 등 통화 관리 싱글톤. SaveService.Data 를 직접 수정하여 세이브와 동기화합니다.
/// </summary>
public class CurrencyService : MonoBehaviour
{
    public static CurrencyService Instance { get; private set; }

    private int _gold;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _gold = SaveService.Data.gold;
    }

    public int Gold => _gold;

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        _gold += amount;
        SaveService.Data.gold = _gold;
        GameEvents.RaiseGoldChanged(_gold);
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (_gold < amount) return false;
        _gold -= amount;
        SaveService.Data.gold = _gold;
        GameEvents.RaiseGoldChanged(_gold);
        return true;
    }
} 