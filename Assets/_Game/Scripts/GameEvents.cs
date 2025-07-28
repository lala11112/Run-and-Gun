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

    // 상태 변경: (IState newState)
    public static event Action<IState> StateChanged;

    // 플레이어 사망
    public static event Action PlayerDied;

    // 적 사망: (bool isBoss)
    public static event Action<bool> EnemyDied;

    // 골드 변경: (int newGold)
    public static event Action<int> GoldChanged;

    // 타이틀 화면 상호작용: (TitleInteractionType type)
    public static event Action<TitleInteractionType> TitleInteractionHovered;

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

    public static void RaiseStateChanged(IState newState)
    {
        StateChanged?.Invoke(newState);
    }

    public static void RaisePlayerDied()
    {
        PlayerDied?.Invoke();
    }

    public static void RaiseEnemyDied(bool isBoss)
    {
        EnemyDied?.Invoke(isBoss);
    }

    public static void RaiseGoldChanged(int newGold)
    {
        GoldChanged?.Invoke(newGold);
    }

    public static void RaiseTitleInteractionHovered(TitleInteractionType type)
    {
        TitleInteractionHovered?.Invoke(type);
    }
} 