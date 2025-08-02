using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// StyleManager의 현재 점수와 랭크를 화면 UI로 표시하는 HUD 스크립트입니다.
/// </summary>
public class StyleHUD : MonoBehaviour
{
    [Header("UI 참조")]
    [Tooltip("랭크를 표시할 TextMeshProUGUI")] public TextMeshProUGUI rankText;
    [Tooltip("스타일 게이지 Slider")] public Slider gaugeSlider;
    [Tooltip("Slider Fill Image – 색상 그라디언트 적용 대상")] public Image gaugeFill;

    [Header("랭크 팝업 설정")]
    [Tooltip("플레이어 머리 위에 띄울 랭크 팝업 프리팹")] public GameObject rankPopupPrefab;
    [Tooltip("플레이어 위치 기준 오프셋")] public Vector3 popupOffset = new Vector3(0f, 1.5f, 0f);
    [Tooltip("팝업이 상승할 높이")] public float popupRise = 1f;
    [Tooltip("팝업 전체 재생 시간")] public float popupDuration = 1f;

    private Transform _player;

    private void Awake()
    {
        // 성능 최적화: FindWithTag 대신 더 효율적인 방법 사용
        var gameConfig = Resources.Load<GameConfigSO>("GameConfig");
        string playerTag = gameConfig != null ? gameConfig.playerTagName : "Player";
        
        var playerObj = GameObject.FindWithTag(playerTag);
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"[StyleHUD] '{playerTag}' 태그를 가진 플레이어를 찾을 수 없습니다!");
        }
    }

    private void OnEnable()
    {
        if (StyleManager.Instance != null)
        {
            StyleManager.Instance.OnRankChanged += HandleRankChanged;
            StyleManager.Instance.OnScoreChanged += HandleScoreChanged;
        }
        // 초기 UI 상태 업데이트
        if (StyleManager.Instance != null)
        {
            HandleRankChanged(StyleManager.Instance.CurrentRank);
            int nextRankScore = StyleManager.Instance.GetScoreForRank(StyleManager.Instance.CurrentRank + 1);
            HandleScoreChanged(StyleManager.Instance.CurrentScore, nextRankScore);
        }
    }

    private void OnDisable()
    {
        if (StyleManager.Instance != null)
        {
            StyleManager.Instance.OnRankChanged -= HandleRankChanged;
            StyleManager.Instance.OnScoreChanged -= HandleScoreChanged;
        }
    }
    
    private void HandleScoreChanged(int currentScore, int nextRankScore)
    {
        if (gaugeSlider == null) return;
        
        StyleRank rank = StyleManager.Instance.CurrentRank;
        int prevRankScore = StyleManager.Instance.GetScoreForRank(rank);

        float segmentLength = nextRankScore - prevRankScore;
        float scoreInSegment = currentScore - prevRankScore;

        gaugeSlider.maxValue = segmentLength > 0 ? segmentLength : 1;
        gaugeSlider.value = scoreInSegment;
    }

    private void HandleRankChanged(StyleRank newRank)
    {
        if (rankText != null)
        {
            rankText.text = $"Rank: {newRank}";
            rankText.color = GetColorForRank(newRank);
        }
        if (gaugeFill != null)
        {
            gaugeFill.color = GetColorForRank(newRank);
        }

        if (newRank > StyleRank.D) // D랭크 달성시에는 팝업 없음
        {
            ShowRankPopup(newRank);
        }
    }
    
    private void ShowRankPopup(StyleRank newRank)
    {
        if (rankPopupPrefab == null || _player == null) return;

        Vector3 startPos = _player.position + popupOffset;
        GameObject obj = Instantiate(rankPopupPrefab, startPos, Quaternion.identity);
        
        // 텍스트 설정
        TMPro.TMP_Text tmp = obj.GetComponentInChildren<TMPro.TMP_Text>(true);
        if (tmp != null)
        {
            tmp.text = newRank.ToString();
            tmp.color = GetColorForRank(newRank);
        }

        // 트윈 시퀀스
        CanvasGroup cg = obj.GetComponent<CanvasGroup>() ?? obj.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        obj.transform.localScale = Vector3.zero;

        cg.DOFade(1f, 0.15f).SetLink(obj);
        obj.transform.DOScale(1.2f, 0.25f).SetEase(Ease.OutBack).SetLink(obj)
            .OnComplete(() =>
            {
                cg.DOFade(0f, 0.3f).SetDelay(popupDuration - 0.45f).SetLink(obj);
            });
        
        obj.transform.DOMoveY(startPos.y + popupRise, popupDuration).SetEase(Ease.OutQuad).SetLink(obj)
            .OnComplete(() => Destroy(obj));
    }

    private Color GetColorForRank(StyleRank rank)
    {
        return rank switch
        {
            StyleRank.D => new Color(0.5f, 0.5f, 0.5f),
            StyleRank.C => new Color(0.3f, 0.9f, 0.3f),
            StyleRank.B => Color.cyan,
            StyleRank.A => new Color(0.9f, 0.4f, 1f),
            StyleRank.S => Color.yellow,
            _ => Color.white,
        };
    }
}
