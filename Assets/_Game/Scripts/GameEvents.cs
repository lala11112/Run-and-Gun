using System;

/// <summary>
/// 간단한 글로벌 이벤트 버스.
/// Manager 싱글톤 의존성을 줄이고, 느슨한 결합으로 상호작용할 수 있도록 돕습니다.
/// </summary>
public static class GameEvents
{
    // 스킬 사용: (SkillType type, bool weakened)
    public static event Action<SkillType, bool> SkillActivated;

    // 스타일 랭크 변경: (StyleRank newRank)
    public static event Action<StyleRank> StyleRankChanged;

    /// <summary>
    /// SkillManager 등에서 호출 – 스킬 사용 이벤트 브로드캐스트
    /// </summary>
    public static void RaiseSkillActivated(SkillType type, bool weakened)
    {
        SkillActivated?.Invoke(type, weakened);
    }

    /// <summary>
    /// StyleManager 등에서 호출 – 랭크 변경 브로드캐스트
    /// </summary>
    public static void RaiseStyleRankChanged(StyleRank newRank)
    {
        StyleRankChanged?.Invoke(newRank);
    }
} 