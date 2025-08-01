using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 모든 구체적인 스킬 로직 클래스들의 부모가 될 추상 클래스입니다.
/// </summary>
public abstract class SkillBase : MonoBehaviour
{
    /// <summary>
    /// 스킬을 발동시키는 핵심 로직입니다.
    /// </summary>
    /// <param name="caster">스킬을 시전한 게임 오브젝트 (플레이어)</param>
    /// <param name="skillData">이 로직이 참조할 스킬의 원본 데이터</param>
    /// <param name="currentRank">스킬 발동 시점의 현재 스타일 랭크</param>
    public abstract void Activate(GameObject caster, SkillDataSO skillData, StyleRank currentRank);
}

// 이 파일은 스킬 시스템에 필요한 모든 '데이터 컨테이너'들을 모아놓은 곳입니다.
// SkillDataSO가 이 클래스들을 부품처럼 사용하여 스킬을 조립합니다.

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

/// <summary>
/// 랭크별 공통 보너스 데이터를 담는 컨테이너입니다.
/// 데미지, 범위, 쿨타임 배율 등 범용적인 보너스를 설정합니다.
/// </summary>
[System.Serializable]
public class RankBonusData
{
    [Tooltip("이 데이터가 적용될 스타일 랭크입니다.")]
    public StyleRank rank;

    [Tooltip("이 랭크에서 스킬의 행동 로직을 특별히 다른 것으로 교체하고 싶을 때 연결합니다. 비워두면 기본 로직을 사용합니다.")]
    public SkillBase overrideLogicPrefab;

    [Header("기본 배율")]
    [Tooltip("기본 데미지에 곱해질 배율입니다. (1 = 100%)")]
    public float damageMultiplier = 1f;
    [Tooltip("기본 쿨타임에 곱해질 배율입니다. (0.8 = 쿨타임 20% 감소)")]
    public float cooldownMultiplier = 1f;

    [Header("스킬별 특수 보너스")]
    [Tooltip("스킬의 공격/탐색 범위에 곱해질 배율입니다.")]
    public float radiusMultiplier = 1f;
    [Tooltip("스킬의 반복/발사 횟수에 곱해질 배율입니다.")]
    public float countMultiplier = 1f;

    [Header("특수 기능 활성화")]
    [Tooltip("이 랭크에서 넉백 기능을 활성화할지 여부입니다.")]
    public bool enableKnockback = false;
    [Tooltip("이 랭크에서 투사체 반사 기능을 활성화할지 여부입니다.")]
    public bool enableProjectileReflection = false;
}

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
    [Tooltip("이동 속도 증가 배율입니다. (1.5 = 50% 증가)")]
    public float moveSpeedModifier = 1f;
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
