using UnityEngine;
using DG.Tweening;
#if EASY_PERFORMANT_OUTLINE
using EPOOutline;
#endif

/// <summary>
/// 타이틀 화면의 배경 오브젝트에 부착하여 특정 상호작용에 반응하게 합니다.
/// 예: '새 게임' 호버 시 문이 빛나는 효과
/// </summary>
public class InteractiveObject : MonoBehaviour
{
    [Tooltip("반응할 상호작용 타입")]
    public TitleInteractionType targetInteraction;

    [Header("색상 변화")]
    [Tooltip("반응 시 사용할 색상")]
    public Color highlightColor = Color.white;
    [Tooltip("반응 지속 시간")]
    public float highlightDuration = 0.5f;

#if EASY_PERFORMANT_OUTLINE
    [Header("아웃라인 설정")]
    [Tooltip("활성화할 아웃라인 컴포넌트. 비워두면 자동으로 찾습니다.")]
    [SerializeField] private Outlinable outlinable;
#endif

    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    private Tween _tween;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _originalColor = _spriteRenderer.color;
        }

#if EASY_PERFORMANT_OUTLINE
        if (outlinable == null)
        {
            outlinable = GetComponent<Outlinable>();
        }

        if (outlinable != null)
        {
            // 시작 시에는 아웃라인 비활성화
            outlinable.enabled = false;
        }
#endif

        GameEvents.TitleInteractionHovered += OnInteraction;
    }

    private void OnDestroy()
    {
        GameEvents.TitleInteractionHovered -= OnInteraction;
        _tween?.Kill();
    }

    private void OnInteraction(TitleInteractionType type)
    {
        _tween?.Kill();

        bool isTargeted = (type == targetInteraction);

        // 색상 변경 처리
        if (_spriteRenderer != null)
        {
            Color targetColor = isTargeted ? highlightColor : _originalColor;
            _tween = _spriteRenderer.DOColor(targetColor, highlightDuration).SetEase(Ease.OutQuad);
        }

#if EASY_PERFORMANT_OUTLINE
        // 아웃라인 처리
        if (outlinable != null)
        {
            outlinable.enabled = isTargeted;
        }
#endif
    }
} 