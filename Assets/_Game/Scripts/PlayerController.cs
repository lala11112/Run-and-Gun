using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using PixelCrushers.DialogueSystem;

/// <summary>
/// 플레이어 캐릭터의 물리적 이동, 회전, 입력 처리를 전담하는 컴포넌트입니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("참조")]
    [Tooltip("플레이어의 기본 스탯이 정의된 ScriptableObject")]
    [SerializeField] private PlayerStatsSO playerStats;

    [Header("입력 액션")]
    [Tooltip("플레이어 이동(Vector2) 액션")]
    public InputActionReference moveAction;

    // 내부 변수들
    private Rigidbody2D _rb;
    private float _baseMoveSpeed; // 스탯 SO에서 가져온 순수 이동 속도
    private float _currentMoveSpeed; // 모든 보너스가 적용된 최종 이동 속도
    private bool _isDashing;
    private Coroutine _speedBuffCoroutine; // 현재 적용중인 속도 버프 코루틴

    /// <summary>
    /// 현재 프레임의 정규화된 입력 방향입니다. (조작 없을 시 0)
    /// </summary>
    public Vector2 CurrentInputDir { get; private set; }
    
    /// <summary>
    /// 마지막으로 0이 아니었던 입력 방향입니다. CSkill 등이 참조합니다.
    /// </summary>
    public Vector2 LastMoveDir { get; private set; }
    
    public bool IsDashing => _isDashing;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (playerStats != null)
        {
            _baseMoveSpeed = playerStats.moveSpeed;
        }
        else
        {
            Debug.LogError("PlayerStatsSO가 연결되지 않았습니다! 기본값으로 설정합니다.", this);
            _baseMoveSpeed = 5f;
        }
        RecalculateMoveSpeed();
        LastMoveDir = Vector2.up; // 초기 방향을 위로 설정
    }

    private void OnEnable()
    {
        moveAction?.action.Enable();
        if (StyleManager.Instance != null)
        {
            StyleManager.Instance.OnRankChanged += HandleRankChange;
        }
    }

    private void OnDisable()
    {
        moveAction?.action.Disable();
        if (StyleManager.Instance != null)
        {
            StyleManager.Instance.OnRankChanged -= HandleRankChange;
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }
    
    private void HandleRankChange(StyleRank newRank)
    {
        RecalculateMoveSpeed();
        Debug.Log($"[PlayerController] 랭크 {newRank} 달성! 이동 속도 다시 계산됨: {_currentMoveSpeed}");
    }
    
    /// <summary>
    /// 스타일 랭크 보너스를 기반으로 현재 이동 속도를 다시 계산합니다.
    /// </summary>
    private void RecalculateMoveSpeed()
    {
        float rankBonus = StyleManager.Instance != null ? StyleManager.Instance.GetCurrentMoveSpeedBonus() : 1f;
        _currentMoveSpeed = _baseMoveSpeed * rankBonus;
    }

    private void HandleMovement()
    {
        if (DialogueManager.IsConversationActive || _isDashing)
        {
            if(!_isDashing) _rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 inputDir = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        CurrentInputDir = inputDir.normalized;

        if (CurrentInputDir.sqrMagnitude > 0.01f)
        {
            LastMoveDir = CurrentInputDir;
        }

        _rb.linearVelocity = CurrentInputDir * _currentMoveSpeed;

        if (LastMoveDir.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(LastMoveDir.y, LastMoveDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }
    }
    
    public void Dash(Vector2 direction, float speed, float duration)
    {
        if (!_isDashing)
        {
            StartCoroutine(DashRoutine(direction, speed, duration));
        }
    }

    private IEnumerator DashRoutine(Vector2 direction, float speed, float duration)
    {
        _isDashing = true;
        Vector2 dir = direction.sqrMagnitude > 0.01f ? direction.normalized : LastMoveDir;
        _rb.linearVelocity = dir * speed;
        yield return new WaitForSeconds(duration);
        _isDashing = false;
        _rb.linearVelocity = Vector2.zero;
    }
    
    /// <summary>
    /// 외부(ZSkillLogic 등)에서 속도 버프를 적용시키는 함수입니다.
    /// </summary>
    public void ApplySpeedBuff(float multiplier, float duration)
    {
        // 기존에 진행중인 버프가 있다면 중지시킵니다.
        if (_speedBuffCoroutine != null)
        {
            StopCoroutine(_speedBuffCoroutine);
        }
        _speedBuffCoroutine = StartCoroutine(SpeedBuffRoutine(multiplier, duration));
    }

    private IEnumerator SpeedBuffRoutine(float multiplier, float duration)
    {
        // 기본 속도에 스타일 보너스와 버프를 모두 적용
        float rankBonus = StyleManager.Instance != null ? StyleManager.Instance.GetCurrentMoveSpeedBonus() : 1f;
        _currentMoveSpeed = _baseMoveSpeed * rankBonus * multiplier;
        
        yield return new WaitForSeconds(duration);
        
        // 버프 시간이 끝나면 스타일 보너스만 적용된 상태로 복구
        RecalculateMoveSpeed();
        _speedBuffCoroutine = null;
    }
}
