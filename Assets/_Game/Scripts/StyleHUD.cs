using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// StyleManager의 현재 점수와 랭크를 화면 UI(TextMeshPro)로 표시하는 간단한 HUD 스크립트.
/// Canvas > TextMeshProUGUI 두 개를 연결하고, 본 스크립트를 Canvas 상에 부착하면 된다.
/// </summary>
public class StyleHUD : MonoBehaviour
{
    [Header("UI 참조")]
    [Tooltip("랭크를 표시할 TextMeshProUGUI")] public TextMeshProUGUI rankText;

    [Tooltip("스타일 게이지 Slider (0 ~ S 랭크 임계값)")] public Slider gaugeSlider;
    [Tooltip("Slider Fill Image – 색상 그라디언트 적용 대상")] public Image gaugeFill;

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
        int curScore = StyleManager.Instance.CurrentScore;

        // 구간 기반 게이지 값 업데이트 (랭크별 0~segmentLength)
        if (gaugeSlider != null)
        {
            int prev = GetPrevThreshold(rank);
            int next = GetNextThreshold(rank);
            int segLen = next - prev;
            gaugeSlider.maxValue = segLen;
            gaugeSlider.value = Mathf.Clamp(curScore - prev, 0, segLen);
        }

        // 색상 그라디언트 (랭크별)
        if (gaugeFill != null)
        {
            gaugeFill.color = GetColorForRank(rank);
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

    // 점수 기준으로 색상을 반환 (D→C→B→A→S 구간에서 점진 변화)
    private Color GetGradientColor(int score)
    {
        // 랭크 컬러 테이블
        Color d = new Color(0.5f,0.5f,0.5f);
        Color c = new Color(0.3f, 0.9f, 0.3f);
        Color b = Color.cyan;
        Color a = new Color(0.9f, 0.4f, 1f);
        Color s = Color.yellow;

        int dT = StyleManager.Instance.dThreshold;
        int cT = StyleManager.Instance.cThreshold;
        int bT = StyleManager.Instance.bThreshold;
        int aT = StyleManager.Instance.aThreshold;
        int sT = StyleManager.Instance.sThreshold;

        if (score < cT)
            return Color.Lerp(d,c, Mathf.InverseLerp(dT,cT,score));
        if (score < bT)
            return Color.Lerp(c,b, Mathf.InverseLerp(cT,bT,score));
        if (score < aT)
            return Color.Lerp(b,a, Mathf.InverseLerp(bT,aT,score));
        if (score < sT)
            return Color.Lerp(a,s, Mathf.InverseLerp(aT,sT,score));
        return s;
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

    private int GetPrevThreshold(StyleRank rank)
    {
        return rank switch
        {
            StyleRank.D => 0,
            StyleRank.C => StyleManager.Instance.cThreshold,
            StyleRank.B => StyleManager.Instance.bThreshold,
            StyleRank.A => StyleManager.Instance.aThreshold,
            _ => StyleManager.Instance.sThreshold, // S 랭크는 꽉 찬 상태 유지
        };
    }

    private int GetNextThreshold(StyleRank rank)
    {
        return rank switch
        {
            StyleRank.D => StyleManager.Instance.cThreshold,
            StyleRank.C => StyleManager.Instance.bThreshold,
            StyleRank.B => StyleManager.Instance.aThreshold,
            StyleRank.A => StyleManager.Instance.sThreshold,
            _ => StyleManager.Instance.sThreshold,
        };
    }
} 