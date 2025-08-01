using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어의 모든 '전투' 관련 행동을 총괄 지휘하는 컨트롤러입니다.
/// 스킬 사용 입력을 받아 StyleManager와 협력하여 스킬 로직을 실행합니다.
/// 이제 스킬의 구체적인 동작은 각 SkillLogic 프리팹이 전적으로 책임집니다.
/// </summary>
[RequireComponent(typeof(PlayerController), typeof(PlayerHealth))]
public class PlayerCombatController : MonoBehaviour
{
    [Header("입력 액션")]
    public InputActionReference attackAction1;
    public InputActionReference attackAction2;
    public InputActionReference attackAction3;
    public InputActionReference dashAction;

    [Header("장착 스킬 데이터")]
    [Tooltip("플레이어가 장착할 스킬 데이터(SO) 목록입니다.")]
    public List<SkillDataSO> equippedSkillDataList = new List<SkillDataSO>();
    
    // 인스턴스화된 스킬 로직(MonoBehaviour) 목록. equippedSkillDataList와 순서가 일치합니다.
    private readonly List<SkillBase> _skillLogics = new List<SkillBase>();

    private void Awake()
    {
        // 장착된 스킬 데이터로부터 실제 로직을 인스턴스화합니다.
        foreach (var skillData in equippedSkillDataList)
        {
            EquipSkill(skillData);
        }
    }

    /// <summary>
    /// 지정된 스킬 데이터를 기반으로 스킬 로직 프리팹을 인스턴스화하고 내부 리스트에 저장합니다.
    /// </summary>
    private void EquipSkill(SkillDataSO skillData)
    {
        if (skillData == null || skillData.logicPrefab == null)
        {
            Debug.LogError($"[PlayerCombatController] 유효하지 않은 스킬 데이터 또는 로직 프리팹입니다: {skillData?.name}");
            return;
        }

        // 스킬 로직 프리팹을 이 오브젝트의 자식으로 인스턴스화합니다.
        SkillBase skillLogicInstance = Instantiate(skillData.logicPrefab, transform);
        skillLogicInstance.name = $"{skillData.skillName}_Logic";
        _skillLogics.Add(skillLogicInstance);
        
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
        // 유효한 슬롯인지, 스킬 로직이 준비되었는지 확인
        if (slotIndex >= _skillLogics.Count || _skillLogics[slotIndex] == null)
        {
            Debug.LogWarning($"{slotIndex + 1}번 공격 슬롯에 스킬이 장착되지 않았거나 로직이 없습니다.");
            return;
        }

        // 1. StyleManager에 스킬 사용 보고 (어떤 스킬인지 식별할 정보 전달)
        // StyleManager가 SO를 직접 참조하도록 변경할 수 있으나, 우선 skillName으로 전달
        if(slotIndex < equippedSkillDataList.Count)
        {
             StyleManager.Instance?.RegisterSkillUsage(equippedSkillDataList[slotIndex]);
        }
       
        // 2. StyleManager로부터 현재 랭크 받아오기
        StyleRank currentRank = StyleManager.Instance != null ? StyleManager.Instance.CurrentRank : StyleRank.D;
        
        // 3. 해당 슬롯의 스킬 로직을 직접 실행
        SkillBase logicToExecute = _skillLogics[slotIndex];
        logicToExecute.Activate(gameObject, currentRank);
        
        Debug.Log($"스킬 '{logicToExecute.name}' 발동 (Rank: {currentRank})");
    }
}
