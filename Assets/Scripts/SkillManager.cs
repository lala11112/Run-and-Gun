using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어의 스킬 시스템을 관리하는 핵심 스크립트
/// Q,W,E,R 스킬의 쿨타임, 연속 사용 패널티, 궁극기 조건을 처리
/// </summary>
public class SkillManager : MonoBehaviour
{
    // 싱글톤 패턴으로 전역 접근 허용
    public static SkillManager Instance { get; private set; }

    [Header("Skill Settings")]
    [Tooltip("4개 스킬(Z,X,C,V)의 설정 데이터 리스트")]
    public List<SkillData> skills = new();

    [Header("Ultimate Settings")]
    [Tooltip("4가지 다른 스킬을 연속 사용해야 하는 시간 제한 (초)")]
    public float comboWindow = 6f;
    
    [Tooltip("궁극기 상태 지속 시간 (초)")]
    public float ultimateDuration = 8f;

    [Header("Input Actions")] 
    [Tooltip("Z 스킬 발동 액션")] public InputActionReference qAction;
    [Tooltip("X 스킬 발동 액션")] public InputActionReference wAction;
    [Tooltip("C 스킬 발동 액션")] public InputActionReference eAction;
    [Tooltip("V 스킬 발동 액션")] public InputActionReference rAction;

    // 이벤트 시스템 - 다른 스크립트에서 스킬 사용/궁극기 상태를 감지할 수 있음
    public System.Action<SkillType, bool /*weakened*/> OnSkillActivated; // 스킬 사용 시 호출

    // 내부 관리 변수들
    private readonly Dictionary<SkillType, float> _cooldowns = new(); // 각 스킬의 남은 쿨타임
    private readonly List<(SkillType type, float time)> _skillHistory = new(); // 스킬 사용 이력
    private Dictionary<SkillType, PlayerSkillBase> _skillBehaviours; // 실제 스킬 동작

    /// <summary>
    /// 궁극기 시스템 제거 — 항상 false 반환
    /// </summary>
    public bool IsUltimateActive => false;

    /// <summary>
    /// 컴포넌트 초기화 및 싱글톤 설정
    /// </summary>
    private void Awake()
    {
        // 싱글톤 패턴 구현 - 중복 인스턴스 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 스킬별 쿨타임 딕셔너리 초기화
        foreach (var s in skills)
        {
            _cooldowns[s.type] = 0f; // 게임 시작 시 모든 스킬 사용 가능
        }

        // 스킬 Behaviour 수집
        _skillBehaviours = new Dictionary<SkillType, PlayerSkillBase>();
        var skillComps = FindObjectsOfType<PlayerSkillBase>();
        foreach (var comp in skillComps)
        {
            if (!_skillBehaviours.ContainsKey(comp.skillType))
                _skillBehaviours.Add(comp.skillType, comp);
        }
    }

    /// <summary>
    /// 매 프레임마다 쿨타임, 궁극기 타이머, 입력 처리
    /// </summary>
    private void Update()
    {
        TickCooldowns(); // 쿨타임 감소 처리
        HandleInput();   // 키보드 입력 처리
    }

    /// <summary>
    /// Q,W,E,R 키 입력을 감지하여 해당 스킬 사용 시도
    /// </summary>
    private void HandleInput()
    {
        if (qAction != null && qAction.action != null && qAction.action.WasPerformedThisFrame()) TryUseSkill(SkillType.Z);
        if (wAction != null && wAction.action != null && wAction.action.WasPerformedThisFrame()) TryUseSkill(SkillType.X);
        if (eAction != null && eAction.action != null && eAction.action.WasPerformedThisFrame()) TryUseSkill(SkillType.C);
        if (rAction != null && rAction.action != null && rAction.action.WasPerformedThisFrame()) TryUseSkill(SkillType.V);
    }

    /// <summary>
    /// 특정 스킬 사용을 시도하는 메서드
    /// 쿨타임 체크, 패널티 계산, 궁극기 조건 확인을 모두 처리
    /// </summary>
    /// <param name="type">사용하려는 스킬 타입</param>
    /// <returns>스킬 사용 성공 여부</returns>
    public bool TryUseSkill(SkillType type)
    {
        // 쿨타임 체크 - 아직 쿨타임이 남아있으면 사용 불가
        if (!_cooldowns.ContainsKey(type) || _cooldowns[type] > 0f) return false;

        bool weakened = false;
        // 연속 사용 패널티는 쿨타임에는 영향 X, 하지만 스킬 효과 약화 등 위해 플래그 유지
        if (_skillHistory.Count > 0 && _skillHistory[_skillHistory.Count - 1].type == type)
        {
            weakened = true;
        }
        
        // 실제 스킬 효과 실행 (Behaviour 있으면)
        if (_skillBehaviours != null && _skillBehaviours.TryGetValue(type, out var behaviour))
        {
            behaviour.RequestActivate(weakened);
        }
        else
        {
            // 이벤트 백워드 호환
            ActivateSkill(type, weakened);
        }
        
        // 쿨타임 설정
        SetCooldown(type, weakened);
        
        // 스킬 사용 이력에 기록
        RecordSkillHistory(type);
        
        return true; // 스킬 사용 성공
    }

    /// <summary>
    /// 실제 스킬 효과를 발동시키는 메서드
    /// 현재는 이벤트만 발생시키며, 구체적인 스킬 효과는 다른 스크립트에서 구현
    /// </summary>
    /// <param name="type">발동할 스킬 타입</param>
    /// <param name="weakened">패널티로 약화된 상태인지</param>
    private void ActivateSkill(SkillType type, bool weakened)
    {
        // TODO: 각 스킬별 구체적인 효과 구현 (이펙트, 피해, 버프 등)
        // 현재는 이벤트를 통해 다른 시스템에 알림
        OnSkillActivated?.Invoke(type, weakened);
    }

    /// <summary>
    /// 스킬의 쿨타임을 설정하는 메서드
    /// 패널티에 따른 쿨타임 증가는 제거되었습니다.
    /// </summary>
    /// <param name="type">쿨타임을 설정할 스킬</param>
    /// <param name="weakened">패널티 적용 여부</param>
    private void SetCooldown(SkillType type, bool weakened)
    {
        // 스킬 데이터에서 기본 쿨타임 가져오기
        var data = skills.Find(s => s.type == type);
        if (data == null) return;
        
        float cd = data.baseCooldown;
        
        // Style rank 기반 쿨타임 배수 적용
        if (StyleManager.Instance != null)
        {
            cd *= StyleManager.Instance.GetCooldownMultiplier();
        }
        
        _cooldowns[type] = cd;
    }

    /// <summary>
    /// 스킬 사용 이력을 기록하고 오래된 기록을 정리
    /// 궁극기 조건 판단에 사용됨
    /// </summary>
    /// <param name="type">사용한 스킬 타입</param>
    private void RecordSkillHistory(SkillType type)
    {
        // 현재 시간과 함께 스킬 사용 기록
        _skillHistory.Add((type, Time.time));
        
        // 콤보 윈도우를 벗어난 오래된 기록들 제거
        _skillHistory.RemoveAll(e => Time.time - e.time > comboWindow);
    }

    /// <summary>
    /// 모든 스킬의 쿨타임을 감소시키는 메서드
    /// </summary>
    private void TickCooldowns()
    {
        var keys = new List<SkillType>(_cooldowns.Keys);
        foreach (var k in keys)
        {
            if (_cooldowns[k] > 0f)
            {
                _cooldowns[k] -= Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// 외부에서 모든 스킬의 쿨타임을 감소시킬 수 있는 공개 메서드
    /// 투사체가 적을 명중했을 때 호출됨 (보상 시스템)
    /// </summary>
    /// <param name="amount">감소시킬 쿨타임 (초)</param>
    public void ReduceAllCooldowns(float amount)
    {
        var keys = new List<SkillType>(_cooldowns.Keys);
        foreach (var k in keys)
        {
            _cooldowns[k] = Mathf.Max(0f, _cooldowns[k] - amount);
        }
    }

    /// <summary>
    /// 특정 스킬의 남은 쿨타임을 조회하는 메서드
    /// UI 시스템에서 쿨타임 표시에 사용
    /// </summary>
    /// <param name="type">조회할 스킬 타입</param>
    /// <returns>남은 쿨타임 (초)</returns>
    public float GetCooldownRemaining(SkillType type)
    {
        return _cooldowns.TryGetValue(type, out var cd) ? cd : 0f;
    }

    /// <summary>
    /// 현재 랭크 등 모든 보정이 적용된 총 쿨타임 값을 반환
    /// </summary>
    public float GetTotalCooldown(SkillType type)
    {
        var data = skills.Find(s => s.type == type);
        if (data == null) return 0f;

        float cd = data.baseCooldown;
        if (StyleManager.Instance != null)
        {
            cd *= StyleManager.Instance.GetCooldownMultiplier();
        }
        return cd;
    }

    private void OnValidate()
    {
        // 인스펙터에서 스킬 데이터가 누락되지 않도록 자동 보완
        foreach (SkillType t in System.Enum.GetValues(typeof(SkillType)))
        {
            if (skills.Find(s => s.type == t) == null)
            {
                skills.Add(new SkillData { type = t, baseCooldown = 3f });
            }
        }
    }
} 