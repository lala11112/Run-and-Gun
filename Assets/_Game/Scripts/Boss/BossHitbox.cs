using UnityEngine;

/// <summary>
/// 플레이어 탄환과 충돌해 데미지를 BossHealth에 전달하는 히트박스.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BossHitbox : MonoBehaviour, IDamageable
{
    [Tooltip("데미지 무시 시간(초) – 피격 후 잠깐 무적용")] public float invincibleTime = 0.2f;
    private float _invTimer = 0f;

    private BossHealth _bossHealth;

    private void Awake()
    {
        _bossHealth = GetComponentInParent<BossHealth>();
        if (TryGetComponent(out Collider2D col)) col.isTrigger = true;

        // 충돌 레이어를 Enemy로 통일하여 PlayerProjectile과의 충돌을 보장합니다.
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) gameObject.layer = enemyLayer;

        // 2D 트리거 충돌을 위해 Rigidbody2D 필요 – 없으면 자동 추가
        if (!TryGetComponent(out Rigidbody2D rb))
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic; // 위치 고정, 물리력 무시
            rb.simulated = true;
            rb.useFullKinematicContacts = false; // 트리거 이벤트만 필요
            rb.gravityScale = 0f;
        }
    }

    private void Update()
    {
        if (_invTimer > 0f) _invTimer -= Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_invTimer > 0f) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerProjectile"))
        {
            TakeDamage(1);
            _invTimer = invincibleTime;
            Destroy(other.gameObject);
        }
    }

    // IDamageable 구현 → BossHealth로 위임
    public void TakeDamage(int dmg)
    {
        _bossHealth?.TakeDamage(dmg);
    }
} 