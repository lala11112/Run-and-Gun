using System.IO;
using UnityEngine;

/// <summary>
/// JSON 직렬화를 이용한 기본 저장 서비스 구현.
/// Easy Save 등 외부 솔루션이 없다면 이 구현을 사용합니다.
/// </summary>
public class JsonSaveService : ISaveService
{
    private readonly string _savePath;

    public JsonSaveService()
    {
        _savePath = Path.Combine(Application.persistentDataPath, "save.json");
    }

    public SaveData Load()
    {
        if (!File.Exists(_savePath))
        {
            Debug.Log("[JsonSaveService] 저장 파일이 없어 기본값을 사용합니다.");
            return new SaveData();
        }

        string json = File.ReadAllText(_savePath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(_savePath, json);
        Debug.Log($"[JsonSaveService] 저장 완료: {_savePath}");
    }
} 