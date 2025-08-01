using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 플레이어의 전투 스타일을 관리하고 랭크를 결정하는 핵심 클래스입니다.
/// 스킬 사용 패턴을 분석하여 점수를 부여하고, 랭크 변경 시 이벤트를 전파합니다.
/// </summary>
public class StyleManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static StyleManager Instance { get; private set; }

    [Header("스타일 점수 설정")]
    [Tooltip("다른 스킬 사용 시 획득하는 점수입니다.")]
    [SerializeField] private int scoreForDifferentSkill = 10;
    [Tooltip("같은 스킬 연속 사용 시 잃는 점수입니다.")]
    [SerializeField] private int penaltyForSameSkill = 5;

    [Header("랭크업 요구 점수")]
    [Tooltip("각 랭크로 올라가기 위해 필요한 누적 점수입니다. (D랭크는 0점)")]
    [SerializeField] private int[] scoreToRankUp = { 0, 50, 120, 250, 400 }; // D, C, B, A, S

    [Header("랭크별 패시브 보너스")]
    [Tooltip("랭크별 이동속도 보너스입니다. (1.1 = 10% 증가)")]
    [SerializeField] private float[] moveSpeedBonusPerRank = { 1.0f, 1.05f, 1.1f, 1.15f, 1.2f };

    // 이벤트: 랭크가 변경될 때 호출됩니다. UI나 다른 시스템이 이 이벤트를 구독합니다.
    public event Action<StyleRank> OnRankChanged;
    // 이벤트: 점수가 변경될 때 호출됩니다.
    public event Action<int, int> OnScoreChanged; // 현재 점수, 다음 랭크까지 필요한 점수

    public StyleRank CurrentRank { get; private set; } = StyleRank.D;
    public int CurrentScore { get; private set; } = 0;

    // 최근 사용한 스킬을 기록하는 리스트
    private List<SkillDataSO> _recentSkills = new List<SkillDataSO>();
    private const int RECENT_SKILL_HISTORY_COUNT = 3; // 최근 스킬 몇 개까지 기억할지

    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않음
    }

    /// <summary>
    /// PlayerCombatController가 스킬을 사용할 때 호출하는 함수입니다.
    /// </summary>
    /// <param name="usedSkill">사용된 스킬의 SkillDataSO</param>
    public void RegisterSkillUsage(SkillDataSO usedSkill)
    {
        if (usedSkill == null) return;

        bool isSameAsLast = _recentSkills.Count > 0 && _recentSkills[_recentSkills.Count - 1] == usedSkill;

        if (isSameAsLast)
        {
            CurrentScore = Mathf.Max(0, CurrentScore - penaltyForSameSkill);
            Debug.Log($"[StyleManager] 같은 스킬 사용! 점수 감소: {CurrentScore}");
        }
        else
        {
            CurrentScore += scoreForDifferentSkill;
            Debug.Log($"[StyleManager] 다른 스킬 사용! 점수 증가: {CurrentScore}");
        }
        
        // 최근 스킬 기록 업데이트
        _recentSkills.Add(usedSkill);
        if (_recentSkills.Count > RECENT_SKILL_HISTORY_COUNT)
        {
            _recentSkills.RemoveAt(0);
        }

        UpdateRank();
        
        int nextRankUpScore = GetScoreForRank(CurrentRank + 1);
        OnScoreChanged?.Invoke(CurrentScore, nextRankUpScore);
    }

    /// <summary>
    /// 현재 점수를 기반으로 랭크를 업데이트합니다.
    /// </summary>
    private void UpdateRank()
    {
        StyleRank newRank = StyleRank.D;
        for (int i = 0; i < scoreToRankUp.Length; i++)
        {
            if (CurrentScore >= scoreToRankUp[i])
            {
                newRank = (StyleRank)i;
            }
            else
            {
                break;
            }
        }
        
        if (newRank != CurrentRank)
        {
            CurrentRank = newRank;
            OnRankChanged?.Invoke(CurrentRank);
            Debug.Log($"[StyleManager] 랭크 변경! 새로운 랭크: {CurrentRank}");
        }
    }
    
    /// <summary>
    /// 특정 랭크에 도달하기 위한 점수를 반환합니다.
    /// </summary>
    public int GetScoreForRank(StyleRank rank)
    {
        int rankIndex = (int)rank;
        if (rankIndex >= 0 && rankIndex < scoreToRankUp.Length)
        {
            return scoreToRankUp[rankIndex];
        }
        return int.MaxValue; // 최고 랭크 이상이면 최대값 반환
    }

    /// <summary>
    /// 현재 랭크에 맞는 이동속도 보너스를 반환합니다.
    /// </summary>
    public float GetCurrentMoveSpeedBonus()
    {
        int rankIndex = (int)CurrentRank;
        if (rankIndex >= 0 && rankIndex < moveSpeedBonusPerRank.Length)
        {
            return moveSpeedBonusPerRank[rankIndex];
        }
        return 1.0f;
    }
}
