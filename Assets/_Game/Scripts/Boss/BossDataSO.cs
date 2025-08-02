using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 보스의 모든 데이터를 정의하는 ScriptableObject입니다.
/// 각 보스마다 고유한 데이터를 가질 수 있습니다.
/// </summary>
[CreateAssetMenu(fileName = "BossData_New", menuName = "Run and Gun/Boss Data", order = 1)]
public class BossDataSO : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("보스의 이름")]
    public string bossName;
    
    [Tooltip("보스 설명")]
    [TextArea]
    public string description;
    
    [Tooltip("보스 아이콘")]
    public Sprite icon;

    [Header("체력 설정")]
    [Tooltip("보스 최대 체력")]
    public int maxHealth = 1000;
    
    [Tooltip("방어력")]
    public int defense = 0;

    [Header("페이즈 설정")]
    [Tooltip("페이즈별 체력 비율 (1.0부터 0.0까지 내림차순)")]
    public List<PhaseData> phases = new List<PhaseData>();

    [Header("보상 설정")]
    [Tooltip("골드 보상 배율 (최대 체력 × 배율)")]
    public float goldRewardMultiplier = 0.1f;
    
    [Tooltip("경험치 보상")]
    public int experienceReward = 100;

    [Header("사운드 설정")]
    [Tooltip("페이즈 변경 시 재생할 사운드")]
    public string phaseChangeSfx = "BossPhase";
    
    [Tooltip("사망 시 재생할 사운드")]
    public string deathSfx = "BossDie";

    [Header("카메라 흔들림 설정")]
    [Tooltip("페이즈 전환 시 사용할 Shake 프리셋 이름")]
    public string phaseShakePreset = "Boss_PhaseChange";
    
    [Tooltip("사망 시 사용할 Shake 프리셋 이름")]
    public string deathShakePreset = "Boss_Death";

    [System.Serializable]
    public class PhaseData
    {
        [Tooltip("페이즈 이름 (예: Phase 1, Berserk Mode)")]
        public string phaseName;
        
        [Tooltip("이 페이즈가 시작되는 체력 비율 (1.0 = 100%, 0.3 = 30%)")]
        [Range(0f, 1f)]
        public float healthRatio = 1f;
        
        [Tooltip("이 페이즈에서 사용할 패턴들")]
        public List<BossPatternSO> patterns = new List<BossPatternSO>();
        
        [Tooltip("페이즈 전환 시 특별한 효과나 대사")]
        public string transitionMessage;
    }

    /// <summary>
    /// 현재 체력 비율에 맞는 페이즈를 반환합니다.
    /// </summary>
    public int GetPhaseForHealthRatio(float healthRatio)
    {
        for (int i = 0; i < phases.Count; i++)
        {
            if (healthRatio >= phases[i].healthRatio)
            {
                return i;
            }
        }
        return phases.Count - 1; // 마지막 페이즈
    }

    /// <summary>
    /// 지정된 페이즈의 데이터를 반환합니다.
    /// </summary>
    public PhaseData GetPhaseData(int phaseIndex)
    {
        if (phaseIndex >= 0 && phaseIndex < phases.Count)
        {
            return phases[phaseIndex];
        }
        return phases.Count > 0 ? phases[0] : null;
    }
}