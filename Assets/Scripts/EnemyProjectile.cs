using UnityEngine;

/// <summary>
/// 적이 발사하는 투사체를 제어하는 스크립트
/// 플레이어에게 피해를 주거나, W 스킬 회전 공격 등에 의해 삭제될 수 있음.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [Tooltip("투사체 속도 (단위/초)")]
    public float speed = 8f;

    [Tooltip("투사체가 플레이어에게 가하는 피해량")]
    public int damage = 1;

    [Tooltip("투사체가 자동으로 파괴되는 시간 (초)")]
    public float lifetime = 5f;

    private Vector2 _direction = Vector2.zero;
    private Rigidbody2D _rb;

    /// <summary>
    /// 발사체 이동 방향 지정 (정규화된 벡터)
    /// </summary>
    public void Init(Vector2 dir)
    {
        _direction = dir.normalized;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        // 수명 타이머 설정
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        // Rigidbody2D 속도 갱신 (FixedUpdate에서 수행)
        if (_direction == Vector2.zero) return;
        _rb.linearVelocity = _direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 동일한 적 또는 다른 투사체와 충돌 시 무시
        if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet")) return;

        // 플레이어와 충돌 체크
        if (other.TryGetComponent(out PlayerController player))
        {
            // TODO: 플레이어 체력 시스템 구현 후 데미지 적용
            // player.TakeDamage(damage);
        }

        // 충돌 후 파괴
        Destroy(gameObject);
    }
} 