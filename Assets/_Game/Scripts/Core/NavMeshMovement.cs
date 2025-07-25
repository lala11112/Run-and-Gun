using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshMovement : MonoBehaviour, IMovement
{
    [Tooltip("이동 속도 (단위/초)")] public float moveSpeed = 2f;
    [Tooltip("플레이어와 유지할 최소 거리")] public float keepDistance = 6f;

    private NavMeshAgent agent;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis   = false;
        agent.speed = moveSpeed;
        agent.stoppingDistance = keepDistance;
    }

    public void Move(Transform target)
    {
        if (agent == null || target == null) return;
        agent.isStopped = false;
        agent.SetDestination(target.position);
    }

    public void Stop()
    {
        if (agent != null) agent.isStopped = true;
    }

    public float MoveSpeed => moveSpeed;
    public float KeepDistance => keepDistance;
} 