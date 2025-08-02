using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 조건에 따라 다른 스텝으로 분기하는 스토리 스텝입니다.
/// </summary>
[CreateAssetMenu(fileName = "ConditionalStep", menuName = "Run and Gun/Story/Steps/Conditional")]
public class ConditionalStepSO : StoryStepSO
{
    [System.Serializable]
    public class ConditionBranch
    {
        [Tooltip("조건 설명 (에디터용)")]
        public string conditionDescription;
        
        [Header("변수 조건")]
        [Tooltip("확인할 변수 이름")]
        public string variableName;
        
        [Tooltip("비교 연산자")]
        public ComparisonOperator comparisonOperator;
        
        [Tooltip("비교할 값 (문자열로 입력, 자동 형변환됩니다)")]
        public string compareValue;
        
        [Header("분기 대상")]
        [Tooltip("조건이 참일 때 실행할 스텝")]
        public StoryStepSO targetStep;
    }
    
    public enum ComparisonOperator
    {
        Equal,          // ==
        NotEqual,       // !=
        Greater,        // >
        GreaterEqual,   // >=
        Less,           // <
        LessEqual,      // <=
        Contains,       // 문자열 포함
        Exists          // 변수 존재 여부
    }
    
    [Header("조건부 분기 설정")]
    [Tooltip("조건들의 목록 (위에서부터 순서대로 확인)")]
    public List<ConditionBranch> conditions = new List<ConditionBranch>();
    
    [Tooltip("모든 조건이 거짓일 때 실행할 기본 스텝")]
    public StoryStepSO defaultStep;

    public override IStoryStepState CreateState(StoryPlayer storyPlayer)
    {
        return new ConditionalState(this, storyPlayer);
    }
}

/// <summary>
/// ConditionalStepSO의 데이터를 받아 실제 로직을 처리하는 상태 클래스입니다.
/// </summary>
public class ConditionalState : IStoryStepState
{
    private readonly ConditionalStepSO _data;
    private readonly StoryPlayer _storyPlayer;

    public ConditionalState(ConditionalStepSO data, StoryPlayer storyPlayer)
    {
        _data = data;
        _storyPlayer = storyPlayer;
    }

    public void Enter()
    {
        // 조건들을 순서대로 확인
        foreach (var condition in _data.conditions)
        {
            if (EvaluateCondition(condition))
            {
                Debug.Log($"[ConditionalState] 조건 '{condition.conditionDescription}' 참 - {condition.targetStep?.name} 실행");
                ExecuteStep(condition.targetStep);
                return;
            }
        }
        
        // 모든 조건이 거짓이면 기본 스텝 실행
        Debug.Log($"[ConditionalState] 모든 조건이 거짓 - 기본 스텝 {_data.defaultStep?.name} 실행");
        ExecuteStep(_data.defaultStep);
    }
    
    private bool EvaluateCondition(ConditionalStepSO.ConditionBranch condition)
    {
        if (string.IsNullOrEmpty(condition.variableName))
        {
            Debug.LogWarning($"[ConditionalState] 변수 이름이 비어있습니다: {condition.conditionDescription}");
            return false;
        }
        
        // 변수 존재 여부만 확인하는 경우
        if (condition.comparisonOperator == ConditionalStepSO.ComparisonOperator.Exists)
        {
            return StoryPlayerContext.HasVariable(condition.variableName);
        }
        
        // 변수가 존재하지 않으면 거짓
        if (!StoryPlayerContext.HasVariable(condition.variableName))
        {
            return false;
        }
        
        var variableValue = StoryPlayerContext.GetVariable<object>(condition.variableName);
        return CompareValues(variableValue, condition.compareValue, condition.comparisonOperator);
    }
    
    private bool CompareValues(object variableValue, string compareValue, ConditionalStepSO.ComparisonOperator op)
    {
        if (variableValue == null) return false;
        
        // 문자열 비교
        if (variableValue is string strValue)
        {
            return op switch
            {
                ConditionalStepSO.ComparisonOperator.Equal => strValue == compareValue,
                ConditionalStepSO.ComparisonOperator.NotEqual => strValue != compareValue,
                ConditionalStepSO.ComparisonOperator.Contains => strValue.Contains(compareValue),
                _ => false
            };
        }
        
        // 숫자 비교
        if (variableValue is int intValue && int.TryParse(compareValue, out int compareInt))
        {
            return op switch
            {
                ConditionalStepSO.ComparisonOperator.Equal => intValue == compareInt,
                ConditionalStepSO.ComparisonOperator.NotEqual => intValue != compareInt,
                ConditionalStepSO.ComparisonOperator.Greater => intValue > compareInt,
                ConditionalStepSO.ComparisonOperator.GreaterEqual => intValue >= compareInt,
                ConditionalStepSO.ComparisonOperator.Less => intValue < compareInt,
                ConditionalStepSO.ComparisonOperator.LessEqual => intValue <= compareInt,
                _ => false
            };
        }
        
        // 실수 비교
        if (variableValue is float floatValue && float.TryParse(compareValue, out float compareFloat))
        {
            return op switch
            {
                ConditionalStepSO.ComparisonOperator.Equal => Mathf.Approximately(floatValue, compareFloat),
                ConditionalStepSO.ComparisonOperator.NotEqual => !Mathf.Approximately(floatValue, compareFloat),
                ConditionalStepSO.ComparisonOperator.Greater => floatValue > compareFloat,
                ConditionalStepSO.ComparisonOperator.GreaterEqual => floatValue >= compareFloat,
                ConditionalStepSO.ComparisonOperator.Less => floatValue < compareFloat,
                ConditionalStepSO.ComparisonOperator.LessEqual => floatValue <= compareFloat,
                _ => false
            };
        }
        
        // 불리언 비교
        if (variableValue is bool boolValue && bool.TryParse(compareValue, out bool compareBool))
        {
            return op switch
            {
                ConditionalStepSO.ComparisonOperator.Equal => boolValue == compareBool,
                ConditionalStepSO.ComparisonOperator.NotEqual => boolValue != compareBool,
                _ => false
            };
        }
        
        return false;
    }
    
    private void ExecuteStep(StoryStepSO step)
    {
        if (step != null)
        {
            var state = step.CreateState(_storyPlayer);
            state.Enter();
        }
        else
        {
            Debug.LogWarning("[ConditionalState] 실행할 스텝이 null입니다. 다음 스텝으로 진행합니다.");
            _storyPlayer.AdvanceToNextStep();
        }
    }

    public void Tick() { }
    public void Exit() { }
}