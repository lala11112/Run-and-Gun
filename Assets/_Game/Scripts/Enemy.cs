using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using DarkTonic.MasterAudio;

/// <summary>
/// 가장 기본적인 적 캐릭터: 플레이어를 추적하고 피해를 받으면 사망.
/// 추후 EnemyShooter 등으로 기능을 확장할 수 있음.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class Enemy : MonoBehaviour
{
    [Header("능력치")]
    // 최대 체력은 EnemyHealth 로 이동

    [Tooltip("적 스탯 ScriptableObject (선택)")] public EnemyStatData statData;

    [Tooltip("이동 속도 (단위/초)")]
    public float moveSpeed = 2f;

    [Header("전투")]
    [Tooltip("플레이어와 유지할 최소 거리")] public float keepDistance = 6f;

    [Tooltip("스턴 시 색상 변경용 SpriteRenderer")] public SpriteRenderer spriteRenderer;
    [Tooltip("스턴 시 컬러")] public Color stunColor = Color.cyan;

    [Header("사망 연출")]
    [Tooltip("사망 사운드의 Sound Group 이름")] public string dieSfx = "EnemyDie";
    [Tooltip("사망 시 카메라 흔들림 지속 시간(초)")] public float dieShakeDuration = 0.25f;
    [Tooltip("사망 시 카메라 흔들림 세기")] public float dieShakeMagnitude = 0.3f;
    [Tooltip("사망 시 적용할 타임스케일 배수 (1 = 정상 속도)")] [Range(0.05f,1f)] public float dieSlowScale = 0.2f;
    [Tooltip("슬로우 모션 지속 시간(실시간 초)")] public float dieSlowDuration = 0.15f;

    private EnemyHealth _health;

    // 체력은 EnemyHealth 컴포넌트로 이동
    public int CurrentHealth => _health != null ? _health.CurrentHealth : 0;
    public int maxHealth => _health != null ? _health.maxHealth : 0; // backward compatibility for HUD

    public event System.Action<int, int> OnHealthChanged
    {
        add { if (_health != null) _health.OnHealthChanged += value; }
        remove { if (_health != null) _health.OnHealthChanged -= value; }
    }

    private Rigidbody2D _rb;
    private NavMeshAgent _agent;
    private Transform _player;

    private bool _isStunned;
    public bool IsStunned => _isStunned;
    private float _stunTimer;
    private Color _originalColor;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _agent = GetComponent<NavMeshAgent>();
        _health = GetComponent<EnemyHealth>();

        // ScriptableObject 스탯이 지정되어 있으면 필드 덮어쓰기
        if (statData != null)
        {
            moveSpeed = statData.moveSpeed;
            keepDistance = statData.keepDistance;
            if (_health != null)
            {
                _health.maxHealth = statData.maxHealth;
                _health.currentHealth = statData.maxHealth;
            }
        }

        _player = GameObject.FindWithTag("Player")?.transform;
        if (spriteRenderer != null) _originalColor = spriteRenderer.color;

        if (_agent != null)
        {
            _agent.speed = moveSpeed; // NavMeshAgent 속도 동기화
            _agent.stoppingDistance = keepDistance; // 유지 거리 설정
            _agent.updateUpAxis = false; // 2D 보정
            _agent.updateRotation = false;
        }

        // Rigidbody는 NavMeshAgent 위치 동기화를 위해 Kinematic 으로 설정하는 것을 권장
        if (_rb != null)
        {
            _rb.bodyType = RigidbodyType2D.Kinematic;
            // NavMeshAgent가 위치를 이동하지만, Collider 감지를 위해 시뮬레이션은 유지합니다.
            _rb.simulated = true;
        }
    }

    private void OnValidate()
    {
        // 에디터 값 변경 시 NavMeshAgent 속도 동기화
        if (_agent == null) _agent = GetComponent<NavMeshAgent>();
        if (_agent != null)
        {
            _agent.speed = moveSpeed;
            _agent.stoppingDistance = keepDistance;
        }
    }

    private void Update()
    {
        HandleStun();
        // 이동 로직은 NavMeshAgent(EnemyNavFollower)에서 처리하므로 여기서 직접 이동하지 않습니다.
    }

    /// <summary>
    /// 외부에서 호출: 피해를 입히고 체력이 0이면 사망.
    /// </summary>
    public void TakeDamage(int dmg)
    {
        // EnemyHealth 컴포넌트가 모든 체력 처리를 담당하도록 위임
        _health?.TakeDamage(dmg);
    }

    /// <summary>
    /// 외부에서 호출: 스턴 부여
    /// </summary>
    public void Stun(float duration)
    {
        _isStunned = true;
        _stunTimer = duration;
        if (_agent != null) _agent.isStopped = true;
        if (spriteRenderer != null) spriteRenderer.color = stunColor;
    }

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

    private void EndStun()
    {
        _isStunned = false;
        if (_agent != null) _agent.isStopped = false;
        if (spriteRenderer != null) spriteRenderer.color = _originalColor;
    }

    // 사망 연출은 EnemyHealth 에서 처리합니다.
} 