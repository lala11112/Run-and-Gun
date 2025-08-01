using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 개별 스킬의 모든 데이터와 행동 방식을 정의하는 ScriptableObject입니다.
/// 이 에셋은 다양한 '데이터 컨테이너'들을 부품으로 조립하여 완성됩니다.
/// 스킬 로직(SkillBase)은 이 데이터를 참조하여 실제 행동을 결정합니다.
/// </summary>
[CreateAssetMenu(fileName = "SkillData_New", menuName = "Run and Gun/Skill Data", order = 1)]
public class SkillDataSO : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("스킬의 이름입니다. UI에 표시될 수 있습니다.")]
    public string skillName;

    [Tooltip("스킬에 대한 상세 설명입니다. UI에 표시될 수 있습니다.")]
    [TextArea]
    public string description;

    [Tooltip("스킬 아이콘으로 사용될 스프라이트입니다.")]
    public Sprite icon;
    
    [Header("핵심 로직 (가장 중요!)")]
    [Tooltip("이 스킬이 기본적으로 사용할 행동 로직 프리팹입니다. SkillBase를 상속받아야 합니다.")]
    public SkillBase baseLogicPrefab;

    [Header("핵심 능력치")]
    [Tooltip("스킬의 기본 재사용 대기시간입니다. (초 단위)")]
    public float baseCooldown = 1f;
    [Tooltip("스킬의 기본 데미지입니다.")]
    public float baseDamage = 10f;
    [Tooltip("스킬의 기본 공격/탐색 반경입니다.")]
    public float baseRadius = 1f;
    [Tooltip("스킬의 기본 발사/반복 횟수입니다.")]
    public int baseCount = 1;


    [Header("랭크별 성장 정보")]
    [Tooltip("각 스타일 랭크에 도달했을 때의 성능 변화 목록입니다.")]
    public List<RankBonusData> rankBonuses = new List<RankBonusData>();


    // --- 아래는 각 스킬의 개성에 따라 선택적으로 사용하는 데이터 컨테이너들입니다. ---
    // 이 스킬에 필요한 데이터 그룹만 값을 채우면 됩니다.
    // 예를 들어, XSkill은 MeleeAttackData를 사용하고, CSkill은 DashData와 ShieldData를 사용합니다.

    [Header("[선택] 근접 공격 데이터")]
    public MeleeAttackData meleeAttackData;

    [Header("[선택] 대시 데이터")]
    public DashData dashData;

    [Header("[선택] 실드 데이터")]
    public ShieldData shieldData;

    [Header("[선택] 투사체 연사 데이터")]
    public BarrageData barrageData;

    [Header("[선택] 자가 버프 데이터")]
    public SelfBuffData selfBuffData;

    [Header("[선택] 점프 & 슬램 데이터")]
    public JumpSlamData jumpSlamData;

    [Header("[선택] 다방향 발사 데이터")]
    public MultiShotData multiShotData;
}
