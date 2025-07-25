#if EASY_SAVE
using UnityEngine;

/// <summary>
/// Easy Save(ES3) 기반 저장 서비스 구현 예시.
/// 실제 Easy Save API 호출로 대체하세요.
/// </summary>
public class EasySaveService : ISaveService
{
    private const string KEY = "SaveData";

    public SaveData Load()
    {
        if (!ES3.KeyExists(KEY))
        {
            Debug.Log("[EasySaveService] 저장 데이터가 없어 기본값을 반환합니다.");
            return new SaveData();
        }
        return ES3.Load(KEY, new SaveData());
    }

    public void Save(SaveData data)
    {
        ES3.Save(KEY, data);
        Debug.Log("[EasySaveService] 저장 완료");
    }
}
#endif 