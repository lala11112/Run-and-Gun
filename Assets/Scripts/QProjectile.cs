using UnityEngine;

/// <summary>
/// Q 스킬로 발사되는 전용 투사체.
/// 적에게 피해를 주고, 적 투사체(EnemyProjectile, EnemyBullet 태그)를 삭제합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class QProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [Tooltip("투사체 속도 (단위/초)")]
    public float speed = 18f;
    [Tooltip("투사체 피해량")]
    public int damage = 1;
    [Tooltip("생존 시간 (초)")]
    public float lifetime = 3f;

    private Vector2 _dir;
    private Rigidbody2D _rb;

    public void Init(Vector2 dir)
    {
        _dir = dir.normalized;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        // 레이어 설정 및 플레이어 충돌 무시
        int projLayer = LayerMask.NameToLayer("PlayerProjectile");
        if (projLayer != -1)
        {
            gameObject.layer = projLayer;
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer != -1)
            {
                Physics2D.IgnoreLayerCollision(projLayer, playerLayer, true);
            }

            // Q 스킬 투사체끼리 충돌 무시 (PlayerProjectile ↔ PlayerProjectile)
            Physics2D.IgnoreLayerCollision(projLayer, projLayer, true);
        }

        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = _dir * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out QTrailZone _))
        {
            return; // Q 장판과는 무시
        }
        // EnemyProjectile 또는 EnemyBullet 태그 제거
        if (other.CompareTag("EnemyBullet"))
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
            return;
        }
        if (other.TryGetComponent(out EnemyProjectile ep))
        {
            Destroy(ep.gameObject);
            Destroy(gameObject);
            return;
        }

        if (other.TryGetComponent(out Enemy enemy))
        {
            enemy.TakeDamage(damage);
            if (StyleManager.Instance != null && StyleManager.Instance.CurrentRank == StyleRank.A)
            {
                enemy.Stun(0.5f);
            }
            StyleManager.Instance?.RegisterSkillHit(SkillType.Q);
            Destroy(gameObject);
            return;
        }

        // 벽 등
        Destroy(gameObject);
    }
} 