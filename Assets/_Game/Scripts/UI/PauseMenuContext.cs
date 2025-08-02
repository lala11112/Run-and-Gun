using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 일시정지 메뉴에 표시될 데이터를 담는 기본 클래스입니다.
/// </summary>
public abstract class PauseMenuContext
{
    public List<SkillDataSO> equippedSkills; // 현재 장착 스킬
}

/// <summary>
/// 로그라이크 모드용 일시정지 메뉴 데이터입니다.
/// </summary>
public class RoguelikePauseContext : PauseMenuContext
{
    public int currentFloor;
    public float playtime;
    public int willpowerEarned; // '의지' 재화
    public StyleRank currentStyleRank;
    public List<ShopItemData> acquiredItems; // 획득한 강화 효과
}

/// <summary>
/// 스토리 모드용 일시정지 메뉴 데이터입니다.
/// </summary>
public class StoryPauseContext : PauseMenuContext
{
    public string currentObjective;
} 