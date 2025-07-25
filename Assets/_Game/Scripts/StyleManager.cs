using UnityEngine;
using DarkTonic.MasterAudio;

/// <summary>
/// 스타일(콤보) 시스템을 관리하는 전역 매니저.
/// 조건: 직전에 적중한 스킬과 다른 스킬이 적중하면 점수가 증가한다.
/// UI 로직은 포함하지 않고, 순수 점수 및 랭크 계산만 제공한다.
/// </summary>
public class StyleManager : MonoBehaviour
{
    public static StyleManager Instance { get; private set; }

    [Header("랭크 설정 (ScriptableObject)")]
    [Tooltip("랭크 관련 임계값 및 배수를 담은 ScriptableObject")] public RankConfig rankConfig;

    [Header("점수 설정")]
    [Tooltip("스킬이 유효하게 적중할 때마다 증가하는 기본 점수")] public int baseScorePerHit = 30;

    [Header("패널티 설정")] [Tooltip("같은 스킬을 연속 사용 시 차감될 점수")] public int repeatSkillPenalty = 20;

    [Header("스킬별 점수 설정")]
    [Tooltip("Z 스킬 적중 시 가산 점수")] public int zScore = 30;
    [Tooltip("X 스킬 적중 시 가산 점수")] public int xScore = 40;
    [Tooltip("C 스킬 적중 시 가산 점수")] public int cScore = 25;
    [Tooltip("V 스킬 적중 시 가산 점수")] public int vScore = 35;

    [Header("랭크 임계값 설정")]
    [Tooltip("D 랭크 임계값")] public int dThreshold = 50;
    [Tooltip("C 랭크 임계값")] public int cThreshold = 100;
    [Tooltip("B 랭크 임계값")] public int bThreshold = 200;
    [Tooltip("A 랭크 임계값")] public int aThreshold = 350;
    [Tooltip("S 랭크 임계값")] public int sThreshold = 600;

    [Header("점수 감소 설정")]
    [Tooltip("행동이 없을 때 점수가 감소하기까지 대기 시간(초)")] public float decayDelay = 2f;
    [Tooltip("점수 감소 속도 (초당)")] public float decayPerSecond = 50f;

    [Header("랭크 버프 설정")]
    [Tooltip("B 랭크에서 쿨타임에 곱해질 배수 (ex 0.9 = 10% 감소)")] public float bCooldownMultiplier = 0.9f;
    [Tooltip("A 랭크에서 쿨타임에 곱해질 배수")] public float aCooldownMultiplier = 0.7f;
    [Tooltip("B 랭크 이동 속도 배수")] public float bMoveSpeedMultiplier = 1.1f;
    [Tooltip("A 랭크 이동 속도 배수")] public float aMoveSpeedMultiplier = 1.25f;
    [Tooltip("S 랭크 쿨타임 배수")] public float sCooldownMultiplier = 0.5f;
    [Tooltip("S 랭크 이동 속도 배수")] public float sMoveSpeedMultiplier = 1.4f;

    [Header("D 랭크 패널티 설정")]
    [Tooltip("D 랭크에서 쿨타임에 곱해질 배수 (1보다 크면 증가)")] public float dCooldownMultiplier = 1.2f;
    [Tooltip("D 랭크 이동 속도 배수 (1보다 작으면 감소)")] public float dMoveSpeedMultiplier = 0.8f;

    [Header("S 랭크 설정")]
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

        // RankConfig SO에서 값 덮어쓰기
        if (rankConfig != null)
        {
            dThreshold = rankConfig.dThreshold;
            cThreshold = rankConfig.cThreshold;
            bThreshold = rankConfig.bThreshold;
            aThreshold = rankConfig.aThreshold;
            sThreshold = rankConfig.sThreshold;

            dCooldownMultiplier = rankConfig.dCooldownMultiplier;
            bCooldownMultiplier = rankConfig.bCooldownMultiplier;
            aCooldownMultiplier = rankConfig.aCooldownMultiplier;
            sCooldownMultiplier = rankConfig.sCooldownMultiplier;

            dMoveSpeedMultiplier = rankConfig.dMoveSpeedMultiplier;
            bMoveSpeedMultiplier = rankConfig.bMoveSpeedMultiplier;
            aMoveSpeedMultiplier = rankConfig.aMoveSpeedMultiplier;
            sMoveSpeedMultiplier = rankConfig.sMoveSpeedMultiplier;

            sDuration = rankConfig.sDuration;
            sExitScore = rankConfig.sExitScore;
        }

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
        // 같은 스킬이 연속으로 적중하면 점수 증가는 없음 (패널티는 SkillManager에서 처리)
        if (_lastSkillHit.HasValue && _lastSkillHit.Value == type)
        {
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
            // 랭크 상승 여부 판단 (숫자가 클수록 높은 랭크)
            bool isRankUp = (int)current > (int)_cachedRank;

            // 캐시된 랭크 업데이트 전에 RankUp 사운드 처리
            if (isRankUp)
            {
                PlayRankUpSFX(current);
            }

            _cachedRank = current;
            OnRankChanged?.Invoke(current);

            // 이벤트 버스 전파
            GameEvents.RaiseStyleRankChanged(current);

            if (current == StyleRank.S)
            {
                _sTimer = sDuration;
            }
        }
    }

    /// <summary>
    /// 랭크가 상승했을 때 호출되어 RankUp 사운드를 재생한다.
    /// 랭크가 높을수록 피치가 조금씩 상승해 더욱 짜릿한 피드백을 준다.
    /// </summary>
    /// <param name="rank">상승 후 랭크</param>
    private void PlayRankUpSFX(StyleRank rank)
    {
        // RankUp이라는 Sound Group을 미리 Master Audio에 세팅해 두어야 합니다.
        // 피치는 D(1.00) → C(1.05) → B(1.10) → A(1.15) → S(1.20)로 점증적으로 상승합니다.
        float pitch = rank switch
        {
            StyleRank.D => 0.8f,
            StyleRank.C => 1.0f,
            StyleRank.B => 1.0f,
            StyleRank.A => 1.2f,
            StyleRank.S => 1.5f,
            _ => 1f
        };

        // 2D 사운드로 재생하여 거리 및 도플러 영향을 제거합니다.
        MasterAudio.PlaySoundAndForget("RankUp", 1.5f, pitch);
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

    /// <summary>
    /// 같은 스킬을 연속 사용했을 때 호출하여 점수를 차감하는 패널티 처리 메서드.
    /// </summary>
    public void ApplyRepeatSkillPenalty()
    {
        _currentScore = Mathf.Max(0, _currentScore - repeatSkillPenalty);
        _decayTimer = decayDelay;
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
            StyleRank.D => dCooldownMultiplier,
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
            StyleRank.D => dMoveSpeedMultiplier,
            _ => 1f,
        };
    }
}

/// <summary>
/// 스타일 랭크 열거형
/// </summary>
public enum StyleRank { D, C, B, A, S } 