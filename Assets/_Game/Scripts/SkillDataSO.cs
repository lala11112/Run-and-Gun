using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 스킬의 '공통 UI 정보'와 '핵심 로직 프리팹'을 정의하는 ScriptableObject입니다.
/// 스킬의 구체적인 동작과 데이터는 각 SkillBase 프리팹과 로직 스크립트에서 직접 관리합니다.
/// </summary>
[CreateAssetMenu(fileName = "SkillData_New", menuName = "Run and Gun/Skill Data", order = 1)]
public class SkillDataSO : ScriptableObject
{
    [Header("UI & 기본 정보")]
    [Tooltip("스킬 식별자 - 스킬의 고유 정보를 담고 있습니다.")]
    public SkillIdentifierSO skillIdentifier;
    
    [Header("핵심 로직 & 쿨타임")]
    [Tooltip("이 스킬이 사용할 행동 로직 프리팹입니다. SkillBase를 상속받아야 합니다.")]
    public SkillBase logicPrefab;

    [Tooltip("스킬의 재사용 대기시간입니다. (초 단위)")]
    public float cooldown = 1f;
}


// ==========================================================================================
// 스킬 시스템에서 공통으로 사용하는 데이터 컨테이너 모음
// ==========================================================================================

/// <summary>
/// 플레이어의 스타일 랭크를 나타냅니다.
/// </summary>
[System.Serializable]
public enum StyleRank
{
    D,
    C,
    B,
    A,
    S
}


// --- 아래 데이터 컨테이너들은 각 스킬 로직 스크립트에서 직접 사용합니다. ---


/// <summary>
/// 근접 범위 공격의 세부 데이터를 담는 컨테이너입니다. (XSkill 등)
/// </summary>
[System.Serializable]
public class MeleeAttackData
{
    [Tooltip("내부(Inner) 범위의 비율입니다. (예: 0.5는 전체 반경의 50%)")]
    public float innerRadiusRatio = 0.5f;
    [Tooltip("내부 범위에 적중 시 추가될 데미지입니다.")]
    public int innerBonusDamage = 0;
    [Tooltip("내부 범위에 적중 시 적을 기절시킬 시간(초)입니다. 0이면 기절 없음.")]
    public float innerStunDuration = 0f;
}

/// <summary>
/// 대시 행동의 세부 데이터를 담는 컨테이너입니다. (CSkill 등)
/// </summary>
[System.Serializable]
public class DashData
{
    [Tooltip("대시 기본 속도입니다.")]
    public float dashSpeed = 25f;
    [Tooltip("대시 지속 시간(거리)입니다.")]
    public float dashDuration = 0.2f;
}

/// <summary>
/// 실드 생성의 세부 데이터를 담는 컨테이너입니다. (CSkill 등)
/// </summary>
[System.Serializable]
public class ShieldData
{
    [Tooltip("생성할 실드 프리팹입니다.")]
    public GameObject shieldPrefab;
    [Tooltip("랭크별 실드의 개수, 각도, 데미지 등을 설정합니다.")]
    public ShieldRankBonus shieldBonus;
}

/// <summary>
/// 랭크에 따라 실드의 성능(개수, 각도 등)을 다르게 설정하기 위한 데이터입니다.
/// </summary>
[System.Serializable]
public class ShieldRankBonus
{
    public int count = 1;
    public float arcAngle = 0f;
    public float lifetime = 1f;
    public float projectileSpeed = 15f;
    public bool applyKnockback = false;
}


/// <summary>
/// 투사체 연사(Barrage)의 세부 데이터를 담는 컨테이너입니다. (ZSkill 등)
/// </summary>
[System.Serializable]
public class BarrageData
{
    [Tooltip("발사할 투사체 프리팹입니다.")]
    public GameObject projectilePrefab;
    [Tooltip("투사체의 속도입니다.")]
    public float projectileSpeed = 20f;
    [Tooltip("투사체의 기본 생존 시간(초)입니다.")]
    public float projectileLifetime = 3f;
    [Tooltip("기본 발사 횟수입니다.")]
    public int projectileCount = 8;
    [Tooltip("발사 간격입니다.")]
    public float fireInterval = 0.07f;
}

/// <summary>
/// 시전자 자신에게 거는 버프의 세부 데이터를 담는 컨테이너입니다. (ZSkill 등)
/// </summary>
[System.Serializable]
public class SelfBuffData
{
    [Tooltip("버프 지속 시간입니다.")]
    public float buffDuration = 3f;
    [Tooltip("이동 속도 증가 배율의 기본값입니다. (1.5 = 50% 증가)")]
    public float baseMoveSpeedModifier = 1.2f;
    [Tooltip("버프 중에 생성할 트레일 이펙트 프리팹입니다.")]
    public GameObject trailPrefab;
}

/// <summary>
/// 점프 후 내리찍는 행동의 세부 데이터를 담는 컨테이너입니다. (VSkill 등)
/// </summary>
[System.Serializable]
public class JumpSlamData
{
    [Tooltip("점프 높이입니다.")]
    public float jumpHeight = 1.2f;
    [Tooltip("공중 체공 시간입니다.")]
    public float airTime = 0.4f;
    [Tooltip("착지 시 카메라 흔들림 프리셋 이름입니다.")]
    public string slamShakePreset;
    [Tooltip("착지 시 생성할 이펙트 프리팹입니다.")]
    public GameObject landingEffectPrefab;
}

/// <summary>
/// 여러 방향으로 동시에 투사체를 발사하는 행동의 세부 데이터를 담는 컨테이너입니다. (VSkill 등)
/// </summary>
[System.Serializable]
public class MultiShotData
{
    [Tooltip("발사할 투사체 프리팹입니다.")]
    public GameObject projectilePrefab;
    [Tooltip("발사할 방향의 개수입니다. (4방, 8방 등)")]
    public int directions = 4;
}
