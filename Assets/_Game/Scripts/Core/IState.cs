/// <summary>
/// 게임 전역 상태 머신에서 사용할 공통 상태 인터페이스입니다.
/// Enter/Exit/Tick 세 가지 메서드만을 제공하여, 각 상태(State) 클래스를
/// 느슨하게 결합시키고 테스트를 용이하게 만듭니다.
/// </summary>
public interface IState
{
    /// <summary>상태에 진입했을 때 호출됩니다.</summary>
    void Enter();

    /// <summary>상태를 벗어날 때 호출됩니다.</summary>
    void Exit();

    /// <summary>매 프레임 갱신 로직을 처리합니다.</summary>
    void Tick();
} 