using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// EnemyCore 파생 보스 래퍼. 이동 모듈이 없으며 데미지는 BossHealth 로 위임된다.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(BossHealth))]
public class BossEnemy : EnemyCore
{
    private BossHealth bossHp;

    protected override void Awake()
    {
        base.Awake();
        bossHp = GetComponent<BossHealth>();
        // 이동 모듈 사용 안 함
        movement = null;

        // 2D NavMesh 사용 시 Z(깊이) 값이 변하지 않도록 NavMeshAgent 설정
        var agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updateUpAxis   = false; // 월드 Z 축 고정
            agent.updateRotation = false; // 에이전트가 회전값을 제어하지 않음
        }
    }

    private void LateUpdate()
    {
        // NavMeshAgent나 기타 요인으로 Z 값이 변했을 경우 다시 0으로 고정합니다.
        if (Mathf.Abs(transform.position.z) > Mathf.Epsilon)
        {
            Vector3 p = transform.position;
            p.z = 0f;
            transform.position = p;
        }
    }

    public override void TakeDamage(int dmg)
    {
        // EnemyCore 내부 HP 사용하지 않고 BossHealth 관리 사용
        bossHp?.TakeDamage(dmg);
    }

    protected override void Die()
    {
        // 실제 사망 로직은 BossHealth.OnBossDead → BossPresentation 의 사망 연출로 처리
    }
} 