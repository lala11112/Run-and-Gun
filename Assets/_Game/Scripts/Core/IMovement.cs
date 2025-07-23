using UnityEngine;

public interface IMovement
{
    void Move(Transform target);
}

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class NavMeshMovement : MonoBehaviour, IMovement
{
    private UnityEngine.AI.NavMeshAgent agent;
    private void Awake()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis   = false;
    }

    public void Move(Transform target)
    {
        if (agent == null || target == null) return;
        agent.isStopped = false;
        agent.SetDestination(target.position);
    }
} 