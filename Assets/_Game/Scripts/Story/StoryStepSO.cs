using UnityEngine;

/// <summary>
/// 모든 스토리 스텝 ScriptableObject의 추상 기반 클래스입니다.
/// 이 클래스는 "어떤 행동을 할 것인가"에 대한 데이터만 정의합니다.
/// </summary>
public abstract class StoryStepSO : ScriptableObject
{
    [Tooltip("에디터에서 스텝을 식별하기 위한 설명")]
    [TextArea]
    public string description;

    /// <summary>
    /// 이 스텝의 데이터를 실행할 구체적인 '상태(State)' 객체를 생성하여 반환합니다.
    /// </summary>
    /// <param name="storyPlayer">이 스텝을 실행하는 주체(엔진)</param>
    /// <returns>생성된 상태 객체</returns>
    public abstract IStoryStepState CreateState(StoryPlayer storyPlayer);
}

/// <summary>
/// 모든 스토리 스텝 상태의 동작을 정의하는 인터페이스입니다.
/// </summary>
public interface IStoryStepState
{
    /// <summary>
    /// 이 상태가 시작될 때 호출됩니다.
    /// 모든 비동기 작업(코루틴)은 여기서 시작되어야 합니다.
    /// </summary>
    void Enter();

    /// <summary>
    /// 이 상태가 매 프레임 실행되어야 할 로직을 담습니다. (필요 없을 수도 있음)
    /// </summary>
    void Tick();
    
    /// <summary>
    /// 이 상태가 종료될 때 호출됩니다.
    /// </summary>
    void Exit();
}
