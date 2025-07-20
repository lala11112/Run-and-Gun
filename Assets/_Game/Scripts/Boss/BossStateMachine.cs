using UnityEngine;

/// <summary>
/// 보스 전투 로직을 제어하는 상태 머신.
/// 게임 오브젝트에 부착하여 사용합니다.
/// </summary>
public class BossStateMachine : MonoBehaviour
{
    [Tooltip("디버그용 – 현재 상태 표시 (읽기 전용)")] public string currentStateName;

    private BossState _currentState;

    public void SetState(BossState newState)
    {
        if (_currentState == newState) return;
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
        currentStateName = _currentState?.GetType().Name ?? "None";
    }

    private void Update()
    {
        _currentState?.Tick();
    }
} 