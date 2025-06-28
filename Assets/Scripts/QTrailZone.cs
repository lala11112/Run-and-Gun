using UnityEngine;

/// <summary>
/// Q 스킬 이동속도 증가 중 생성되는 트레일 장판.
/// 일정 시간 유지되며 닿은 적에게 1회 데미지를 준다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class QTrailZone : MonoBehaviour
{
    [Tooltip("장판이 가하는 데미지")] public int damage = 1;
    [Tooltip("장판 유지 시간(초)")] public float lifetime = 1.5f;

    private void Awake()
    {
        // 트리거 설정
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        // 플레이어 투사체와 충돌 무시를 위해 동일 레이어 지정
        int projLayer = LayerMask.NameToLayer("PlayerProjectile");
        if (projLayer != -1) gameObject.layer = projLayer;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어 투사체와는 상호작용하지 않음
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerProjectile"))
            return;

        if (other.TryGetComponent(out Enemy enemy))
        {
            enemy.TakeDamage(damage);
        }
    }
} 