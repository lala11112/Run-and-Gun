using System.Collections;
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

    public Vector2 CurrentInputDir { get; private set; }
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
        _currentMoveSpeed = _baseMoveSpeed;
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
        float bonus = StyleManager.Instance.GetCurrentMoveSpeedBonus();
        _currentMoveSpeed = _baseMoveSpeed * bonus;
        Debug.Log($"[PlayerController] 랭크 {newRank} 달성! 이동 속도 보너스 적용: {_currentMoveSpeed} ({bonus * 100}%)");
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

        _rb.linearVelocity = CurrentInputDir * _currentMoveSpeed;

        if (CurrentInputDir.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(CurrentInputDir.y, CurrentInputDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }
    }
    
    /// <summary>
    /// 외부(CSkillLogic)에서 대시를 시작시키는 함수입니다.
    /// </summary>
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
        Vector2 dir = direction.sqrMagnitude > 0.01f ? direction.normalized : (Vector2)transform.up;
        _rb.linearVelocity = dir * speed;

        yield return new WaitForSeconds(duration);

        _isDashing = false;
        _rb.linearVelocity = Vector2.zero;
    }
}
