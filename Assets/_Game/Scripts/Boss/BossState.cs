using UnityEngine;

/// <summary>
/// 보스 상태 머신이 관리하는 개별 상태의 추상 베이스 클래스.
/// </summary>
public abstract class BossState
{
    /// <summary>상태 진입 시 1회 호출</summary>
    public abstract void Enter();
    /// <summary>매 프레임 호출</summary>
    public abstract void Tick();
    /// <summary>상태 종료 시 1회 호출</summary>
    public abstract void Exit();
} 