using UnityEngine;

/// <summary>
/// R 스킬로 발사되는 거대한 투사체.
/// 적에게 피해를 주고, 지나가는 경로상의 모든 적 투사체(EnemyProjectile, EnemyBullet 태그)를 삭제한다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class GiantProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [Tooltip("이동 속도 (단위/초)")] public float speed = 15f;
    [Tooltip("데미지") ] public int damage = 3;
    [Tooltip("생존 시간(초)")] public float lifetime = 4f;

    private Vector2 _dir;
    private Rigidbody2D _rb;

    public void Init(Vector2 dir)
    {
        _dir = dir.normalized;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (_dir == Vector2.zero) return;
        _rb.linearVelocity = _dir * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 적 투사체 제거
        if (other.CompareTag("EnemyBullet"))
        {
            Destroy(other.gameObject);
            return; // 거대 투사체는 계속 진행
        }
        if (other.TryGetComponent(out EnemyProjectile ep))
        {
            Destroy(ep.gameObject);
            return;
        }

        // 적에게 피해
        if (other.TryGetComponent(out Enemy enemy))
        {
            enemy.TakeDamage(damage);
        }
    }
} 