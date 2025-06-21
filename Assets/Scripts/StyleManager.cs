using UnityEngine;

/// <summary>
/// 스타일(콤보) 시스템을 관리하는 전역 매니저.
/// 조건: 직전에 적중한 스킬과 다른 스킬이 적중하면 점수가 증가한다.
/// UI 로직은 포함하지 않고, 순수 점수 및 랭크 계산만 제공한다.
/// </summary>
public class StyleManager : MonoBehaviour
{
    public static StyleManager Instance { get; private set; }

    [Header("Score Settings")]
    [Tooltip("스킬이 유효하게 적중할 때마다 증가하는 기본 점수")] public int baseScorePerHit = 30;

    [Header("Rank Thresholds")]
    [Tooltip("B 랭크가 되기 위한 점수 임계값")] public int bThreshold = 150;
    [Tooltip("A 랭크가 되기 위한 점수 임계값")] public int aThreshold = 300;

    [Header("Decay Settings")]
    [Tooltip("행동이 없을 때 점수가 감소하기까지 대기 시간(초)")] public float decayDelay = 2f;
    [Tooltip("점수 감소 속도 (초당)")] public float decayPerSecond = 50f;

    private int _currentScore;
    private SkillType? _lastSkillHit; // 마지막으로 적중한 스킬
    private float _decayTimer;

    public int CurrentScore => _currentScore;
    public StyleRank CurrentRank => GetRankByScore(_currentScore);

    public System.Action<StyleRank> OnRankChanged; // 랭크 변화 이벤트

    private StyleRank _cachedRank = StyleRank.C;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (_decayTimer > 0f)
        {
            _decayTimer -= Time.deltaTime;
        }
        else if (_currentScore > 0)
        {
            _currentScore = Mathf.Max(0, _currentScore - Mathf.RoundToInt(decayPerSecond * Time.deltaTime));
        }

        CheckRankChange();
    }

    /// <summary>
    /// 외부에서 스킬이 적에게 적중했을 때 호출하는 메서드
    /// </summary>
    /// <param name="type">적중한 스킬 타입</param>
    public void RegisterSkillHit(SkillType type)
    {
        // 같은 스킬이 연속으로 적중하면 점수 증가 없음
        if (_lastSkillHit.HasValue && _lastSkillHit.Value == type)
        {
            // 갱신만 하고 종료
            _decayTimer = decayDelay;
            return;
        }

        _lastSkillHit = type;
        _currentScore += baseScorePerHit;
        _decayTimer = decayDelay;

        CheckRankChange();
    }

    private StyleRank GetRankByScore(int score)
    {
        if (score >= aThreshold) return StyleRank.A;
        if (score >= bThreshold) return StyleRank.B;
        return StyleRank.C;
    }

    private void CheckRankChange()
    {
        StyleRank current = GetRankByScore(_currentScore);
        if (current != _cachedRank)
        {
            _cachedRank = current;
            OnRankChanged?.Invoke(current);
        }
    }
}

/// <summary>
/// 스타일 랭크 열거형
/// </summary>
public enum StyleRank { C, B, A } 