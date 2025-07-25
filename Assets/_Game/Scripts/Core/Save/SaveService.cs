/// <summary>
/// 저장 시스템 퍼사드 – 런타임에서 한 번 초기화 후 언제든 접근할 수 있도록 합니다.
/// </summary>
public static class SaveService
{
    private static ISaveService _impl;
    public static SaveData Data { get; private set; }

    /// <summary>
    /// 부트 단계에서 호출하여 저장 시스템을 초기화합니다.
    /// Easy Save 에셋이 존재하면 해당 구현으로, 그렇지 않으면 JSON 구현을 사용합니다.
    /// </summary>
    public static void Initialize()
    {
#if EASY_SAVE
        _impl = new EasySaveService();
#else
        _impl = new JsonSaveService();
#endif
        Data = _impl.Load();
    }

    public static void Save()
    {
        _impl?.Save(Data);
    }
} 