using UnityEngine;

/// <summary>
/// 기존 Enemy 기능과 호환되는 래퍼 – Enemy 프리팹을 단계적으로 이 클래스로 교체할 수 있다.
/// </summary>
[RequireComponent(typeof(NavMeshMovement))]
public class SimpleEnemy : EnemyCore
{
    protected override void Die()
    {
        // TODO: 사망 이펙트/사운드 – 기존 EnemyHealth.Die 연출을 이관
        Destroy(gameObject);
    }
} 