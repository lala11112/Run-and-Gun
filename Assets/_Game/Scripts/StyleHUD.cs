using UnityEngine;
using DG.Tweening;
using TMPro;

/// <summary>
/// StyleManager의 현재 점수와 랭크를 화면 UI(TextMeshPro)로 표시하는 간단한 HUD 스크립트.
/// Canvas > TextMeshProUGUI 두 개를 연결하고, 본 스크립트를 Canvas 상에 부착하면 된다.
/// </summary>
public class StyleHUD : MonoBehaviour
{
    [Header("UI 참조")]
    [Tooltip("점수를 표시할 TextMeshProUGUI")] public TextMeshProUGUI scoreText;
    [Tooltip("랭크를 표시할 TextMeshProUGUI")] public TextMeshProUGUI rankText;

    [Header("랭크 팝업 설정")]
    [Tooltip("플레이어 머리 위에 띄울 랭크 팝업 프리팹 (TextMeshProUGUI 포함)")] public GameObject rankPopupPrefab;
    [Tooltip("플레이어 위치 기준 오프셋")] public Vector3 popupOffset = new Vector3(0f, 1.5f, 0f);
    [Tooltip("팝업이 상승할 높이")] public float popupRise = 1f;
    [Tooltip("팝업 전체 재생 시간")] public float popupDuration = 1f;

    private Transform _player;

    private void Awake()
    {
        _player = GameObject.FindWithTag("Player")?.transform;
    }

    private void OnEnable()
    {
        GameEvents.StyleRankChanged += HandleRankChanged;
    }

    private void OnDisable()
    {
        GameEvents.StyleRankChanged -= HandleRankChanged;
    }

    private void Update()
    {
        if (StyleManager.Instance == null) return;

        StyleRank rank = StyleManager.Instance.CurrentRank;

        // 점수 표시 (S 랭크일 때는 숨김)
        if (scoreText != null)
        {
            if (rank == StyleRank.S)
                scoreText.gameObject.SetActive(false);
            else
            {
                scoreText.gameObject.SetActive(true);
            scoreText.text = $"Score: {StyleManager.Instance.CurrentScore}";
            }
        }

        // 랭크 표시 + 색상
        if (rankText != null)
        {
            rankText.text = $"Rank: {rank}";
            rankText.color = GetColorForRank(rank);
        }
    }

    private Color GetColorForRank(StyleRank rank)
    {
        return rank switch
        {
            StyleRank.D => new Color(0.5f,0.5f,0.5f),
            StyleRank.C => new Color(0.3f, 0.9f, 0.3f),
            StyleRank.B => Color.cyan,
            StyleRank.A => new Color(0.9f, 0.4f, 1f),
            _ => Color.white,
        };
    }

    private void HandleRankChanged(StyleRank newRank)
    {
        if (rankPopupPrefab == null || _player == null) return;

        Vector3 startPos = _player.position + popupOffset;
        // 팝업 프리팹은 World Space Canvas 권장. Canvas가 Overlay인 경우 화면 좌표로 변환.
        GameObject obj = Instantiate(rankPopupPrefab, Vector3.zero, Quaternion.identity);

        // RectTransform 위치 결정
        if (obj.TryGetComponent(out RectTransform rt))
        {
            Canvas popupCanvas = obj.GetComponentInParent<Canvas>();
            if (popupCanvas != null && popupCanvas.renderMode != RenderMode.WorldSpace)
            {
                // Overlay / ScreenSpaceCamera: 월드 → 스크린 좌표 변환
                Vector2 screenPos = Camera.main.WorldToScreenPoint(startPos);
                rt.position = screenPos;
            }
            else
            {
                // WorldSpace: 직접 월드 좌표 사용
                obj.transform.position = startPos;
            }
        }
        else
        {
            obj.transform.position = startPos;
        }

        // 텍스트 설정 (프리팹 내부 어디에 있어도 검색)
        TMPro.TMP_Text tmp = obj.GetComponentInChildren<TMPro.TMP_Text>(true);
        if (tmp != null)
        {
            tmp.text = newRank.ToString();
            tmp.color = GetColorForRank(newRank);
        }

        // CanvasGroup 확보
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        obj.transform.localScale = Vector3.zero;

        // 트윈 시퀀스
        cg.DOFade(1f, 0.15f).SetLink(obj)
            .OnComplete(() => cg.DOFade(0f, 0.3f).SetDelay(popupDuration - 0.3f).SetLink(obj));

        obj.transform.DOScale(1.2f, 0.25f).SetEase(Ease.OutBack).SetLink(obj);
        obj.transform.DOMoveY(startPos.y + popupRise, popupDuration).SetEase(Ease.OutQuad).SetLink(obj)
            .OnComplete(() => Destroy(obj));
    }
} 