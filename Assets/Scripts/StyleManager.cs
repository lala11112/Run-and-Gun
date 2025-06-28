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

    [Header("스킬별 점수 설정")]
    [Tooltip("Z 스킬 적중 시 가산 점수")] public int zScore = 30;
    [Tooltip("X 스킬 적중 시 가산 점수")] public int xScore = 40;
    [Tooltip("C 스킬 적중 시 가산 점수")] public int cScore = 25;
    [Tooltip("V 스킬 적중 시 가산 점수")] public int vScore = 35;

    [Header("Rank Thresholds (수치 이상이 되면 해당 랭크)")]
    [Tooltip("D 랭크 임계값")] public int dThreshold = 50;
    [Tooltip("C 랭크 임계값")] public int cThreshold = 100;
    [Tooltip("B 랭크 임계값")] public int bThreshold = 200;
    [Tooltip("A 랭크 임계값")] public int aThreshold = 350;
    [Tooltip("S 랭크 임계값")] public int sThreshold = 600;

    [Header("Decay Settings")]
    [Tooltip("행동이 없을 때 점수가 감소하기까지 대기 시간(초)")] public float decayDelay = 2f;
    [Tooltip("점수 감소 속도 (초당)")] public float decayPerSecond = 50f;

    [Header("Rank Buff Settings")]
    [Tooltip("B 랭크에서 쿨타임에 곱해질 배수 (ex 0.9 = 10% 감소)")] public float bCooldownMultiplier = 0.9f;
    [Tooltip("A 랭크에서 쿨타임에 곱해질 배수")] public float aCooldownMultiplier = 0.7f;
    [Tooltip("B 랭크 이동 속도 배수")] public float bMoveSpeedMultiplier = 1.1f;
    [Tooltip("A 랭크 이동 속도 배수")] public float aMoveSpeedMultiplier = 1.25f;
    [Tooltip("S 랭크 쿨타임 배수")] public float sCooldownMultiplier = 0.5f;
    [Tooltip("S 랭크 이동 속도 배수")] public float sMoveSpeedMultiplier = 1.4f;

    [Header("S Rank Settings")]
    [Tooltip("S 랭크 유지 시간(초)")] public float sDuration = 8f;
    [Tooltip("S 랭크 종료 후 설정될 점수")] public int sExitScore = 400;

    private int _currentScore;
    private SkillType? _lastSkillHit; // 마지막으로 적중한 스킬
    private float _decayTimer;
    private float _sTimer;

    public int CurrentScore => _currentScore;
    public StyleRank CurrentRank => GetRankByScore(_currentScore);

    public System.Action<StyleRank> OnRankChanged; // 랭크 변화 이벤트

    private StyleRank _cachedRank = StyleRank.C;

    public float SRemainingNormalized => (_cachedRank == StyleRank.S && sDuration > 0f) ? Mathf.Clamp01(_sTimer / sDuration) : 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 게임 시작 시 C 랭크 점수로 초기화
        _currentScore = cThreshold;
    }

    private void Update()
    {
        if (_decayTimer > 0f)
        {
            _decayTimer -= Time.deltaTime;
        }
        else if (_currentScore > 0 && _cachedRank >= StyleRank.B)
        {
            _currentScore = Mathf.Max(0, _currentScore - Mathf.RoundToInt(decayPerSecond * Time.deltaTime));
        }

        // S 랭크 유지 시간 처리
        if (_cachedRank == StyleRank.S)
        {
            _sTimer -= Time.deltaTime;
            if (_sTimer <= 0f)
            {
                // 강제 점수 조정 후 랭크 재계산
                _currentScore = sExitScore;
                CheckRankChange();
            }
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
        int add = baseScorePerHit;
        add = type switch
        {
            SkillType.Z => zScore,
            SkillType.X => xScore,
            SkillType.C => cScore,
            SkillType.V => vScore,
            _ => baseScorePerHit
        };

        _currentScore += add;
        _decayTimer = decayDelay;

        CheckRankChange();
    }

    private StyleRank GetRankByScore(int score)
    {
        if (score >= sThreshold) return StyleRank.S;
        if (score >= aThreshold) return StyleRank.A;
        if (score >= bThreshold) return StyleRank.B;
        if (score >= cThreshold) return StyleRank.C;
        return StyleRank.D;
    }

    private void CheckRankChange()
    {
        StyleRank current = GetRankByScore(_currentScore);
        if (current != _cachedRank)
        {
            _cachedRank = current;
            OnRankChanged?.Invoke(current);

            if (current == StyleRank.S)
            {
                _sTimer = sDuration;
            }
        }
    }

    /// <summary>
    /// 스타일 점수를 소모합니다. 최소 0으로 클램프 후 랭크 변동 체크.
    /// </summary>
    /// <param name="amount">소모할 점수(양수)</param>
    public void ConsumeScore(int amount)
    {
        if (amount <= 0) return;
        _currentScore = Mathf.Max(0, _currentScore - amount);
        _decayTimer = decayDelay; // 소모 시 디케이 타이머 초기화
        CheckRankChange();
    }

    // --- Public helper methods for other systems ---
    public float GetCooldownMultiplier()
    {
        return _cachedRank switch
        {
            StyleRank.S => sCooldownMultiplier,
            StyleRank.A => aCooldownMultiplier,
            StyleRank.B => bCooldownMultiplier,
            _ => 1f,
        };
    }

    public float GetMoveSpeedMultiplier()
    {
        return _cachedRank switch
        {
            StyleRank.S => sMoveSpeedMultiplier,
            StyleRank.A => aMoveSpeedMultiplier,
            StyleRank.B => bMoveSpeedMultiplier,
            _ => 1f,
        };
    }
}

/// <summary>
/// 스타일 랭크 열거형
/// </summary>
public enum StyleRank { D, C, B, A, S } 