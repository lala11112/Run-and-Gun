using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 스토리 컨텍스트 변수를 설정하는 스텝입니다.
/// </summary>
[CreateAssetMenu(fileName = "SetVariableStep", menuName = "Run and Gun/Story/Steps/Set Variable")]
public class SetVariableStepSO : StoryStepSO
{
    [System.Serializable]
    public class VariableSetting
    {
        [Tooltip("설정할 변수의 이름")]
        public string variableName;
        
        [Tooltip("변수의 타입")]
        public VariableType variableType;
        
        [Tooltip("설정할 값 (문자열로 입력, 자동 형변환됩니다)")]
        public string value;
        
        [Tooltip("기존 값에 더할지 여부 (숫자 타입만 해당)")]
        public bool addToExisting = false;
    }
    
    public enum VariableType
    {
        String,
        Integer,
        Float,
        Boolean
    }
    
    [Header("변수 설정")]
    [Tooltip("설정할 변수들의 목록")]
    public List<VariableSetting> variableSettings = new List<VariableSetting>();

    public override IStoryStepState CreateState(StoryPlayer storyPlayer)
    {
        return new SetVariableState(this, storyPlayer);
    }
}

/// <summary>
/// SetVariableStepSO의 데이터를 받아 실제 로직을 처리하는 상태 클래스입니다.
/// </summary>
public class SetVariableState : IStoryStepState
{
    private readonly SetVariableStepSO _data;
    private readonly StoryPlayer _storyPlayer;

    public SetVariableState(SetVariableStepSO data, StoryPlayer storyPlayer)
    {
        _data = data;
        _storyPlayer = storyPlayer;
    }

    public void Enter()
    {
        foreach (var setting in _data.variableSettings)
        {
            SetVariable(setting);
        }
        
        // 모든 변수 설정 완료 후 다음 스텝으로
        _storyPlayer.AdvanceToNextStep();
    }
    
    private void SetVariable(SetVariableStepSO.VariableSetting setting)
    {
        if (string.IsNullOrEmpty(setting.variableName))
        {
            Debug.LogWarning("[SetVariableState] 변수 이름이 비어있습니다.");
            return;
        }
        
        try
        {
            switch (setting.variableType)
            {
                case SetVariableStepSO.VariableType.String:
                    SetStringVariable(setting);
                    break;
                    
                case SetVariableStepSO.VariableType.Integer:
                    SetIntegerVariable(setting);
                    break;
                    
                case SetVariableStepSO.VariableType.Float:
                    SetFloatVariable(setting);
                    break;
                    
                case SetVariableStepSO.VariableType.Boolean:
                    SetBooleanVariable(setting);
                    break;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SetVariableState] 변수 '{setting.variableName}' 설정 중 오류: {e.Message}");
        }
    }
    
    private void SetStringVariable(SetVariableStepSO.VariableSetting setting)
    {
        StoryPlayerContext.SetVariable(setting.variableName, setting.value);
        Debug.Log($"[SetVariableState] 문자열 변수 '{setting.variableName}' = '{setting.value}'");
    }
    
    private void SetIntegerVariable(SetVariableStepSO.VariableSetting setting)
    {
        if (int.TryParse(setting.value, out int intValue))
        {
            if (setting.addToExisting)
            {
                int currentValue = StoryPlayerContext.GetVariable<int>(setting.variableName, 0);
                intValue += currentValue;
            }
            
            StoryPlayerContext.SetVariable(setting.variableName, intValue);
            Debug.Log($"[SetVariableState] 정수 변수 '{setting.variableName}' = {intValue}");
        }
        else
        {
            Debug.LogError($"[SetVariableState] '{setting.value}'를 정수로 변환할 수 없습니다.");
        }
    }
    
    private void SetFloatVariable(SetVariableStepSO.VariableSetting setting)
    {
        if (float.TryParse(setting.value, out float floatValue))
        {
            if (setting.addToExisting)
            {
                float currentValue = StoryPlayerContext.GetVariable<float>(setting.variableName, 0f);
                floatValue += currentValue;
            }
            
            StoryPlayerContext.SetVariable(setting.variableName, floatValue);
            Debug.Log($"[SetVariableState] 실수 변수 '{setting.variableName}' = {floatValue}");
        }
        else
        {
            Debug.LogError($"[SetVariableState] '{setting.value}'를 실수로 변환할 수 없습니다.");
        }
    }
    
    private void SetBooleanVariable(SetVariableStepSO.VariableSetting setting)
    {
        if (bool.TryParse(setting.value, out bool boolValue))
        {
            StoryPlayerContext.SetVariable(setting.variableName, boolValue);
            Debug.Log($"[SetVariableState] 불리언 변수 '{setting.variableName}' = {boolValue}");
        }
        else
        {
            Debug.LogError($"[SetVariableState] '{setting.value}'를 불리언으로 변환할 수 없습니다.");
        }
    }

    public void Tick() { }
    public void Exit() { }
}