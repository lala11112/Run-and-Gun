using UnityEngine;

/// <summary>
/// 플레이어 탄환과 충돌해 데미지를 BossHealth에 전달하는 히트박스.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BossHitbox : MonoBehaviour
{
    [Tooltip("데미지 무시 시간(초) – 피격 후 잠깐 무적용")] public float invincibleTime = 0.2f;
    private float _invTimer = 0f;

    private BossHealth _bossHealth;

    private void Awake()
    {
        _bossHealth = GetComponentInParent<BossHealth>();
        if (TryGetComponent(out Collider2D col)) col.isTrigger = true;
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
            _bossHealth?.TakeDamage(1);
            _invTimer = invincibleTime;
            Destroy(other.gameObject);
        }
    }
} 