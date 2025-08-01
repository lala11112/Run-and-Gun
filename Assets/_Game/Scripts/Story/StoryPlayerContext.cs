using UnityEngine;

/// <summary>
/// 스토리 재생 중에 여러 스텝에 걸쳐 공유되어야 하는 데이터를 담는 정적 클래스입니다.
/// </summary>
public static class StoryPlayerContext
{
    /// <summary>
    /// 현재 씬에 생성되어 있는 맵 프리팹의 인스턴스입니다.
    /// </summary>
    public static GameObject CurrentMap { get; set; }

    /// <summary>
    /// 컨텍스트를 초기 상태로 리셋합니다. 챕터 시작 시 호출될 수 있습니다.
    /// </summary>
    public static void Reset()
    {
        if (CurrentMap != null)
        {
            Object.Destroy(CurrentMap);
        }
        CurrentMap = null;
    }
}
