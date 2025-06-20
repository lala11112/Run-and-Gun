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
    [Tooltip("대시 속도 (단위/초)")]
    public float dashSpeed = 20f;
    [Tooltip("대시 지속 시간 (초)")]
    public float dashDuration = 0.2f;

    [Header("Input Actions")]
    [Tooltip("플레이어 이동(V2) 액션 (Input System)")]
    public InputActionReference moveAction;

    // 내부 변수들
    private float _fireTimer; // 발사 간격 계산용 타이머
    private Rigidbody2D _rb; // 2D 물리 컴포넌트
    private Camera _mainCam; // 메인 카메라 참조
    private bool _isDashing; // 대시 중엔 일반 이동 입력 무시

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
        _mainCam = Camera.main;
    }

    /// <summary>
    /// 매 프레임마다 플레이어 동작 처리
    /// </summary>
    private void Update()
    {
        HandleMovement(); // 이동 처리
        HandleAiming();   // 조준 처리
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
        
        // Rigidbody2D의 velocity를 직접 설정하여 이동
        _rb.linearVelocity = inputDir * moveSpeed;
    }
    #endregion

    #region Aiming
    /// <summary>
    /// 마우스 위치를 기준으로 플레이어 회전 처리
    /// 2D 환경에서 Z축 회전을 사용하여 마우스 방향을 바라보도록 함
    /// </summary>
    private void HandleAiming()
    {
        // 메인 카메라가 없으면 처리하지 않음
        if (_mainCam == null) return;

        // 마우스 스크린 좌표를 월드 좌표로 변환
        Vector3 mouseWorld = _mainCam.ScreenToWorldPoint(Input.mousePosition);
        
        // 플레이어에서 마우스 위치로의 방향 벡터 계산
        Vector2 dir = mouseWorld - transform.position;
        
        // 방향 벡터가 유효한지 확인 (너무 작으면 회전하지 않음)
        if (dir.sqrMagnitude > 0.001f)
        {
            // 2D에서 각도 계산 (atan2 사용)
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            
            // Z축 회전 적용 (스프라이트가 위쪽을 향한다고 가정하여 -90도 보정)
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
    /// 대시 시작. speedMultiplier 값을 통해 거리를 조절할 수 있음
    /// </summary>
    /// <param name="speedMultiplier">dashSpeed에 곱해지는 배수</param>
    public void StartDash(Vector2 direction, float speedMultiplier = 1f)
    {
        if (!_isDashing)
        {
            StartCoroutine(DashRoutine(direction, speedMultiplier));
        }
    }

    private IEnumerator DashRoutine(Vector2 direction, float speedMultiplier)
    {
        _isDashing = true;

        Vector2 dir = direction.sqrMagnitude > 0.01f ? direction.normalized : transform.up;
        _rb.linearVelocity = dir * dashSpeed * speedMultiplier;

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
} 