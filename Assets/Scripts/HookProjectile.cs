using UnityEngine;

/// <summary>
/// R 스킬 갈고리 투사체. 적 또는 벽 충돌 시 PlayerSkills에 콜백.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class HookProjectile : MonoBehaviour
{
    [HideInInspector] public PlayerSkills owner;
    public float speed = 20f;
    public float maxDistance = 10f;

    private Vector2 _dir;
    private Vector2 _startPos;
    private Rigidbody2D _rb;
    private bool _latched;

    public void Init(Vector2 dir)
    {
        _dir = dir.normalized;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _startPos = transform.position;
    }

    private void FixedUpdate()
    {
        if (_latched) return;
        _rb.linearVelocity = _dir * speed;
        if (Vector2.Distance(_startPos, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_latched) return;
        if (owner == null)
        {
            Destroy(gameObject);
            return;
        }

        // Enemy hit
        if (other.TryGetComponent(out Enemy enemy))
        {
            _latched = true;
            _rb.linearVelocity = Vector2.zero;
            owner.OnHookHitEnemy(enemy);
            Destroy(gameObject);
            return;
        }

        // Wall hit (Layer "Wall" or Tag "Wall")
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall") || other.CompareTag("Wall"))
        {
            _latched = true;
            _rb.linearVelocity = Vector2.zero;
            owner.OnHookHitWall(transform.position);
            // projectile stays? destroy for now
            Destroy(gameObject);
            return;
        }
    }
} 