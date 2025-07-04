using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NavMesh 에이전트를 이용해 플레이어(또는 지정된 타겟)를 지속적으로 추적하는 간단한 컴포넌트입니다.
/// 2D 프로젝트이므로 Z 회전과 Up 축 갱신을 비활성화하여 XY 평면에서만 이동하도록 합니다.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavFollower : MonoBehaviour
{
    [Header("타겟")]
    [Tooltip("추적할 대상 Transform. 비워두면 태그가 'Player' 인 오브젝트를 자동으로 찾습니다.")]
    [SerializeField] private Transform target;

    [Header("추적 범위")]
    [Tooltip("플레이어를 추적할 최대 거리(단위: 유닛). 0 이하이면 추적하지 않음.")]
    public float followRange = 10f;

    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // 2D 설정: Z 회전 및 Y-Up 축 갱신 비활성화
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        // 타겟이 비어 있으면 런타임에 플레이어를 찾아 설정
        if (target == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                target = playerObj.transform;
        }
    }

    private void Update()
    {
        // 타겟이 유효하지 않으면 아무것도 하지 않음
        if (target == null)
        {
            if (agent != null) agent.isStopped = true;
            return;
        }

        // 추적 범위가 설정되어 있고, 일정 거리 이상이면 추적 중단
        if (followRange <= 0f)
        {
            if (agent != null) agent.isStopped = true;
            return; // 추적 비활성화
        }

        float dist = Vector2.Distance(transform.position, target.position);
        if (dist > followRange)
        {
            if (agent != null) agent.isStopped = true;
            return;
        }

        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
        }
    }
} 