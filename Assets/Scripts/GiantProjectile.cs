using UnityEngine;

/// <summary>
/// R 스킬로 발사되는 거대한 투사체.
/// 적에게 피해를 주고, 지나가는 경로상의 모든 적 투사체(EnemyProjectile, EnemyBullet 태그)를 삭제한다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class GiantProjectile : MonoBehaviour
{
    [Header("투사체 설정")]
    [Tooltip("이동 속도 (단위/초)")] public float speed = 15f;
    [Tooltip("데미지") ] public int damage = 3;
    [Tooltip("생존 시간(초)")] public float lifetime = 4f;

    [Header("추가 효과")]
    [Tooltip("적중 시 적을 기절시키는지 여부")] public bool stunOnHit = false;
    [Tooltip("기절 지속 시간")] public float stunDuration = 0.8f;
    [Tooltip("적중 시 적을 끌어당기는지 여부")] public bool pullOnHit = false;
    [Tooltip("끌어당기는 힘(Impulse)")] public float pullForce = 10f;

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

        // 벽 태그 처리: 환경에 충돌 시 즉시 파괴 (Wall/Obstacle)
        if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
            return;
        }

        // 적에게 피해
        if (other.TryGetComponent(out Enemy enemy))
        {
            enemy.TakeDamage(damage);

            // 기절 효과
            if (stunOnHit)
            {
                enemy.Stun(stunDuration);
            }

            // 끌어당김 효과
            if (pullOnHit && enemy.TryGetComponent(out Rigidbody2D erb))
            {
                Vector2 dir = (Vector2)transform.position - erb.position;
                erb.AddForce(dir.normalized * pullForce, ForceMode2D.Impulse);
            }
        }

        // 기타 오브젝트에 충돌 시 파괴 (플레이어나 투사체 등은 제외)
        if (!other.CompareTag("Enemy") && !other.CompareTag("EnemyBullet") && !other.TryGetComponent<EnemyProjectile>(out _))
        {
            Destroy(gameObject);
        }
    }
} 