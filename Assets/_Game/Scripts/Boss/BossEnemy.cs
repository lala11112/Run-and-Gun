using UnityEngine;

/// <summary>
/// 보스용 Enemy 파생 클래스.
/// 이동 AI를 비활성화하여 NavMeshAgent 경로 탐색을 사용하지 않고,
/// TestBattleManager 에서는 Enemy 로 인식되도록만 기능을 유지합니다.
/// </summary>
public class BossEnemy : Enemy
{
    private void Awake()
    {
        // base Enemy.Awake() 이미 실행됨 (Unity는 상속 체인의 모든 Awake 호출)
        // 보스는 자리 고정 – NavMeshAgent 중지
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.isStopped = true;

        // Enemy.Awake 에서 moveSpeed 적용 전에 필드를 덮어쓰면 무시되므로 Start 에서 보정
    }

    private void Start()
    {
        moveSpeed = 0f;
    }
} 