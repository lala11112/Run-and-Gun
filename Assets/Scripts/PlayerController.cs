using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 캐릭터를 제어하는 스크립트
/// WASD 이동, 마우스 조준, 좌클릭 연사 기능을 담당
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("플레이어 이동 속도 (단위/초)")]
    public float moveSpeed = 6f;

    [Header("Aiming")]
    [Tooltip("마우스 레이캐스트가 충돌할 레이어마스크 (예: 지면 레이어)")]
    public LayerMask aimLayerMask;

    [Header("Shooting")]
    [Tooltip("투사체가 발사될 위치 (플레이어의 자식 오브젝트)")]
    public Transform firePoint;
    [Tooltip("발사할 투사체 프리팹")]
    public GameObject projectilePrefab;
    [Tooltip("연사 속도 (초당 발사 횟수)")]
    public float fireRate = 10f;

    [Header("Dash")]
    [Tooltip("대시 지속 시간 (초)")]
    public float dashDuration = 0.2f;

    [Header("Input Actions")]
    [Tooltip("플레이어 이동(V2) 액션 (Input System)")]
    public InputActionReference moveAction;

    // 내부 변수들
    private float _fireTimer; // 발사 간격 계산용 타이머
    private Rigidbody2D _rb; // 2D 물리 컴포넌트
    private bool _isDashing; // 대시 중 이동 입력 무시
    private Vector2 _lastLookDir = Vector2.up; // 마지막으로 바라본 방향 (회전 유지)

    /// <summary>
    /// 최근 프레임에 입력된 이동 방향(정규화). 0,0 이면 입력 없음.
    /// </summary>
    public Vector2 CurrentInputDir { get; private set; }

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    private void Awake()
    {
        // 필요한 컴포넌트들 가져오기
        _rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 매 프레임마다 플레이어 동작 처리
    /// </summary>
    private void Update()
    {
        HandleMovement(); // 이동 및 회전 처리 (마우스 조준 제거)
        HandleShooting(); // 사격 처리
    }

    #region Movement
    /// <summary>
    /// WASD 키 입력을 받아 플레이어 이동 처리
    /// Rigidbody2D의 velocity를 직접 조작하여 부드러운 이동 구현
    /// </summary>
    private void HandleMovement()
    {
        if (_isDashing) return; // 대시 중엔 수동 입력 무시

        Vector2 inputDir = Vector2.zero;
        if (moveAction != null && moveAction.action != null)
        {
            inputDir = moveAction.action.ReadValue<Vector2>();
        }

        inputDir = inputDir.normalized;
        CurrentInputDir = inputDir;
        
        float rankMoveMult = StyleManager.Instance != null ? StyleManager.Instance.GetMoveSpeedMultiplier() : 1f;
        // Rigidbody2D의 velocity를 직접 설정하여 이동 (랭크 이동 속도 보정 포함)
        _rb.linearVelocity = inputDir * moveSpeed * rankMoveMult;

        // 회전 처리: 입력이 있으면 방향 갱신, 없으면 이전 방향 유지
        if (inputDir.sqrMagnitude > 0.001f)
        {
            _lastLookDir = inputDir;
        }

        // 항상 _lastLookDir 기준으로 회전 유지
        if (_lastLookDir.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(_lastLookDir.y, _lastLookDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }
    }
    #endregion

    #region Shooting
    /// <summary>
    /// 마우스 좌클릭 입력을 받아 연사 처리
    /// fireRate에 따라 발사 간격을 조절
    /// </summary>
    private void HandleShooting()
    {
        // 필수 컴포넌트가 없으면 처리하지 않음
        if (projectilePrefab == null || firePoint == null) return;

        // 마우스 좌클릭 상태 확인
        bool isFiring = Input.GetMouseButton(0);
        
        // 발사 타이머 업데이트
        _fireTimer += Time.deltaTime;
        
        // 발사 간격 계산 (fireRate를 기준으로)
        float timeBetweenShots = 1f / fireRate;

        // 발사 조건 확인: 마우스를 누르고 있고, 발사 간격이 충분히 지났을 때
        if (isFiring && _fireTimer >= timeBetweenShots)
        {
            _fireTimer = 0f; // 타이머 초기화
            Shoot(); // 발사 실행
        }
    }

    /// <summary>
    /// 실제 투사체 발사를 처리하는 메서드
    /// firePoint 위치에서 투사체를 생성하고 방향을 설정
    /// </summary>
    private void Shoot()
    {
        // 투사체 프리팹을 firePoint 위치와 회전으로 생성
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        
        // 생성된 투사체에 Projectile 컴포넌트가 있다면 초기화
        if (proj.TryGetComponent(out Projectile projectile))
        {
            // firePoint의 up 방향으로 투사체 방향 설정 (2D에서 up이 전방)
            projectile.Init(firePoint.up);
        }
    }
    #endregion

    /// <summary>
    /// 대시 시작. 절대 속도를 직접 전달하여 PlayerController는 이동만 처리하도록 단일 책임화.
    /// </summary>
    /// <param name="speed">대시 속도 (단위/초)</param>
    public void StartDash(Vector2 direction, float speed)
    {
        if (!_isDashing)
        {
            StartCoroutine(DashRoutine(direction, speed));
        }
    }

    private IEnumerator DashRoutine(Vector2 direction, float speed)
    {
        _isDashing = true;

        Vector2 dir = direction.sqrMagnitude > 0.01f ? direction.normalized : transform.up;
        _rb.linearVelocity = dir * speed;

        // 대시 동안 충돌 무시 / 무적 처리는 이후 구현 가능
        yield return new WaitForSeconds(dashDuration);

        _isDashing = false;
        _rb.linearVelocity = Vector2.zero; // 대시 종료 후 정지 (원활한 이동 전환을 위해)
    }

    /// <summary>
    /// 외부에서 플레이어 Rigidbody2D에 접근할 수 있도록 하는 프로퍼티
    /// </summary>
    public Rigidbody2D Rigidbody2D => _rb;

    /// <summary>
    /// 외부 스크립트가 임펄스 힘을 적용할 수 있도록 헬퍼 메서드 제공
    /// </summary>
    public void AddImpulse(Vector2 force)
    {
        _rb.AddForce(force, ForceMode2D.Impulse);
    }

    public bool IsDashing => _isDashing;
} 