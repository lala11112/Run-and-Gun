using System.Collections;
using UnityEngine;

/// <summary>
/// 가장 기본적인 적 캐릭터: 플레이어를 추적하고 피해를 받으면 사망.
/// 추후 EnemyShooter 등으로 기능을 확장할 수 있음.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [Tooltip("최대 체력")]
    public int maxHealth = 3;

    [Tooltip("이동 속도 (단위/초)")]
    public float moveSpeed = 2f;

    [Tooltip("스턴 시 색상 변경용 SpriteRenderer")] public SpriteRenderer spriteRenderer;
    [Tooltip("스턴 시 컬러")] public Color stunColor = Color.cyan;

    private int _currentHealth;
    private Rigidbody2D _rb;
    private Transform _player;

    private bool _isStunned;
    private float _stunTimer;
    private Color _originalColor;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _player = GameObject.FindWithTag("Player")?.transform;
        _currentHealth = maxHealth;
        if (spriteRenderer != null) _originalColor = spriteRenderer.color;
    }

    private void Update()
    {
        HandleStun();
        HandleMovement();
    }

    /// <summary>
    /// 플레이어를 향해 이동 (스턴 시 멈춤)
    /// </summary>
    private void HandleMovement()
    {
        if (_player == null || _isStunned) return;

        Vector2 dir = (_player.position - transform.position).normalized;
        _rb.linearVelocity = dir * moveSpeed;
    }

    /// <summary>
    /// 스턴 타이머 처리
    /// </summary>
    private void HandleStun()
    {
        if (_isStunned)
        {
            _stunTimer -= Time.deltaTime;
            if (_stunTimer <= 0f)
            {
                EndStun();
            }
        }
    }

    /// <summary>
    /// 외부에서 호출: 피해를 입히고 체력이 0이면 사망.
    /// </summary>
    public void TakeDamage(int dmg)
    {
        _currentHealth -= dmg;
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 외부에서 호출: 스턴 부여
    /// </summary>
    public void Stun(float duration)
    {
        _isStunned = true;
        _stunTimer = duration;
        _rb.linearVelocity = Vector2.zero;
        if (spriteRenderer != null) spriteRenderer.color = stunColor;
    }

    private void EndStun()
    {
        _isStunned = false;
        if (spriteRenderer != null) spriteRenderer.color = _originalColor;
    }

    private void Die()
    {
        // TODO: 폭발 이펙트, 점수 추가 등.
        Destroy(gameObject);
    }
} 