using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 구체적인 스타일 로직 클래스들의 부모가 될 추상 클래스입니다.
/// </summary>
public abstract class StyleBase : MonoBehaviour
{
    /// <summary>
    /// 스타일이 활성화될 때 호출되는 로직입니다.
    /// </summary>
    /// <param name="caster">스타일을 사용하는 게임 오브젝트입니다. (플레이어)</param>
    /// <param name="styleData">이 로직이 참조할 스타일의 원본 데이터입니다.</param>
    public abstract void OnStyleActivated(GameObject caster, StyleDataSO styleData);

    /// <summary>
    /// 스타일이 비활성화될 때 호출되는 로직입니다.
    /// </summary>
    /// <param name="caster">스타일을 사용했던 게임 오브젝트입니다. (플레이어)</param>
    /// <param name="styleData">이 로직이 참조할 스타일의 원본 데이터입니다.</param>
    public abstract void OnStyleDeactivated(GameObject caster, StyleDataSO styleData);
}

/// <summary>
/// 스타일의 모든 데이터와 규칙을 정의하는 ScriptableObject입니다.
/// </summary>
[CreateAssetMenu(fileName = "StyleData_New", menuName = "Run and Gun/Style Data", order = 2)]
public class StyleDataSO : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("스타일의 이름입니다.")]
    public string styleName;

    [Tooltip("스타일 아이콘입니다.")]
    public Sprite icon;

    [Header("핵심 규칙")]
    [Tooltip("이 스타일이 활성화되었을 때 장착할 스킬들의 목록입니다.")]
    public List<SkillDataSO> equippedSkills = new List<SkillDataSO>();

    [Tooltip("이 스타일이 사용할 실제 행동 로직을 담고 있는 프리팹입니다. 해당 프리팹에는 StyleBase를 상속받은 스크립트가 있어야 합니다.")]
    public StyleBase styleLogicPrefab;
    
    // 여기에 스타일별 스탯 보너스 등을 추가할 수 있습니다.
    [Header("스탯 보너스")]
    [Tooltip("이 스타일 활성화 시 추가될 방어력입니다.")]
    public float bonusDefense = 0f;
}
