using UnityEngine;
using UnityEngine.AI;

public interface IStunnable
{
    bool IsStunned { get; }
    void Stun(float duration);
}

/// <summary>
/// 기본 스턴 처리 모듈. 필요할 때만 부착하여 사용.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class StunHandler : MonoBehaviour, IStunnable
{
    [Tooltip("스턴 중 NavMeshAgent 정지 여부")] public bool stopMovement = true;
    private bool _isStunned;
    private float _timer;
    private NavMeshAgent _agent;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    public bool IsStunned => _isStunned;

    public void Stun(float duration)
    {
        _isStunned = true;
        _timer = duration;
        if (stopMovement && _agent != null) _agent.isStopped = true;
    }

    private void Update()
    {
        if (!_isStunned) return;
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _isStunned = false;
            if (stopMovement && _agent != null) _agent.isStopped = false;
        }
    }
} 