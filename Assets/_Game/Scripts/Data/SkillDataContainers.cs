using UnityEngine;

/// <summary>
/// 스킬의 고유 식별자를 나타내는 ScriptableObject입니다.
/// enum 대신 이를 사용하여 더 유연하고 확장 가능한 스킬 시스템을 구현합니다.
/// </summary>
[CreateAssetMenu(fileName = "SkillIdentifier_New", menuName = "Run and Gun/Skills/Skill Identifier", order = 1)]
public class SkillIdentifierSO : ScriptableObject
{
    [Header("스킬 식별 정보")]
    [Tooltip("스킬의 고유 ID (예: Z, X, C, V, FireBall, IceSpike 등)")]
    public string skillId;
    
    [Tooltip("스킬의 표시 이름")]
    public string displayName;
    
    [Tooltip("스킬 설명")]
    [TextArea]
    public string description;
    
    [Tooltip("스킬 아이콘")]
    public Sprite icon;
    
    [Header("분류")]
    [Tooltip("스킬 카테고리 (예: Combat, Movement, Support)")]
    public string category = "Combat";
    
    [Tooltip("스킬 태그들 (검색 및 필터링용)")]
    public string[] tags;

    /// <summary>
    /// 스킬 ID로 비교합니다.
    /// </summary>
    public bool Equals(SkillIdentifierSO other)
    {
        return other != null && skillId == other.skillId;
    }

    /// <summary>
    /// 스킬 ID를 반환합니다.
    /// </summary>
    public override string ToString()
    {
        return skillId;
    }
}

/// <summary>
/// 모든 구체적인 스킬 로직 클래스들의 부모가 될 추상 클래스입니다.
/// </summary>
public abstract class SkillBase : MonoBehaviour
{
    /// <summary>
    /// 스킬을 발동시키는 핵심 로직입니다.
    /// 이제 각 스킬 로직은 자기 자신의 데이터를 직접 참조합니다.
    /// </summary>
    /// <param name="caster">스킬을 시전한 게임 오브젝트 (플레이어)</param>
    /// <param name="currentRank">스킬 발동 시점의 현재 스타일 랭크</param>
    public abstract void Activate(GameObject caster, StyleRank currentRank);
}

// 참고: 기존에 있던 다른 데이터 컨테이너들은 SkillDataSO.cs 파일로 이동하여 관리합니다.
