using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어의 모든 '전투' 관련 행동을 총괄 지휘하는 컨트롤러입니다.
/// 스킬 사용 입력을 받아 StyleManager와 협력하여 스킬 로직을 실행합니다.
/// </summary>
[RequireComponent(typeof(PlayerController), typeof(PlayerHealth))]
public class PlayerCombatController : MonoBehaviour
{
    [Header("입력 액션")]
    public InputActionReference attackAction1;
    public InputActionReference attackAction2;
    public InputActionReference attackAction3;
    public InputActionReference dashAction;

    [Header("테스트용 스킬 데이터")]
    [Tooltip("테스트를 위해 인스펙터에서 직접 연결할 스킬 데이터 목록입니다.")]
    public List<SkillDataSO> testSkillDatas = new List<SkillDataSO>();

    // 현재 장착된 스킬의 원본 데이터
    private List<SkillDataSO> _equippedSkillDatas = new List<SkillDataSO>();

    // 스킬 데이터와 인스턴스화된 로직을 매핑하는 딕셔너리
    private Dictionary<SkillDataSO, SkillBase> _skillLogicInstances = new Dictionary<SkillDataSO, SkillBase>();

    private PlayerController _playerController;
    private PlayerHealth _playerHealth;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _playerHealth = GetComponent<PlayerHealth>();

        // 테스트 스킬 장착
        foreach (var skillData in testSkillDatas)
        {
            EquipSkill(skillData);
        }
    }

    /// <summary>
    /// 지정된 스킬 데이터를 기반으로 스킬 로직을 인스턴스화하고 장착합니다.
    /// </summary>
    private void EquipSkill(SkillDataSO skillData)
    {
        if (skillData == null || skillData.baseLogicPrefab == null)
        {
            Debug.LogError($"[PlayerCombatController] 유효하지 않은 스킬 데이터 또는 베이스 로직 프리팹입니다: {skillData?.name}");
            return;
        }

        // 이미 장착된 스킬인지 확인
        if (_skillLogicInstances.ContainsKey(skillData)) return;

        // 스킬 로직 프리팹을 이 오브젝트의 자식으로 인스턴스화합니다.
        SkillBase skillLogicInstance = Instantiate(skillData.baseLogicPrefab, transform);
        skillLogicInstance.name = $"{skillData.skillName}_Logic";

        _equippedSkillDatas.Add(skillData);
        _skillLogicInstances.Add(skillData, skillLogicInstance);

        Debug.Log($"스킬 '{skillData.skillName}'이(가) 장착되었습니다.");
    }

    private void OnEnable()
    {
        if (attackAction1 != null) attackAction1.action.performed += OnAttack1;
        if (attackAction2 != null) attackAction2.action.performed += OnAttack2;
        if (attackAction3 != null) attackAction3.action.performed += OnAttack3;
        if (dashAction != null) dashAction.action.performed += OnDash;
    }

    private void OnDisable()
    {
        if (attackAction1 != null) attackAction1.action.performed -= OnAttack1;
        if (attackAction2 != null) attackAction2.action.performed -= OnAttack2;
        if (attackAction3 != null) attackAction3.action.performed -= OnAttack3;
        if (dashAction != null) dashAction.action.performed -= OnDash;
    }
    
    // 각 공격/스킬 입력에 대한 핸들러
    private void OnAttack1(InputAction.CallbackContext context) => TryActivateSkill(0);
    private void OnAttack2(InputAction.CallbackContext context) => TryActivateSkill(1);
    private void OnAttack3(InputAction.CallbackContext context) => TryActivateSkill(2);
    private void OnDash(InputAction.CallbackContext context) => TryActivateSkill(3);

    /// <summary>
    /// 지정된 슬롯의 스킬을 발동시킵니다.
    /// </summary>
    /// <param name="slotIndex">스킬 슬롯 인덱스</param>
    private void TryActivateSkill(int slotIndex)
    {
        if (slotIndex >= _equippedSkillDatas.Count || _equippedSkillDatas[slotIndex] == null)
        {
            Debug.LogWarning($"{slotIndex + 1}번 공격 슬롯에 스킬이 장착되지 않았습니다.");
            return;
        }

        SkillDataSO skillToActivate = _equippedSkillDatas[slotIndex];
        
        // 1. StyleManager에 스킬 사용 보고
        StyleManager.Instance?.RegisterSkillUsage(skillToActivate);

        // 2. StyleManager로부터 현재 랭크 받아오기
        StyleRank currentRank = StyleManager.Instance != null ? StyleManager.Instance.CurrentRank : StyleRank.D;

        // 3. 랭크에 맞는 스킬 로직 찾기 (오버라이드 로직이 있다면 그것을 사용)
        SkillBase logicToExecute = GetLogicForRank(skillToActivate, currentRank);
        
        // 4. 스킬 로직 실행
        logicToExecute.Activate(gameObject, skillToActivate, currentRank);
    }
    
    /// <summary>
    /// 스킬 데이터와 현재 랭크에 맞는 실제 스킬 로직(SkillBase) 인스턴스를 반환합니다.
    /// </summary>
    private SkillBase GetLogicForRank(SkillDataSO skillData, StyleRank rank)
    {
        // 랭크별 오버라이드 로직이 있는지 확인
        foreach (var rankBonus in skillData.rankBonuses)
        {
            if (rankBonus.rank == rank && rankBonus.overrideLogicPrefab != null)
            {
                // TODO: 오버라이드 로직 인스턴스화 및 관리 필요
                // 지금은 간단하게 기본 로직을 반환
                break;
            }
        }

        // 오버라이드 로직이 없으면, 기본으로 장착된 로직 인스턴스를 반환
        if (_skillLogicInstances.TryGetValue(skillData, out SkillBase baseLogic))
        {
            return baseLogic;
        }

        Debug.LogError($"실행할 스킬 로직을 찾을 수 없습니다: {skillData.name}");
        return null;
    }
}
