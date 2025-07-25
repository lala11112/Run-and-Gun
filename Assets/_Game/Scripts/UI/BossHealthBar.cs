using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 화면 하단 고정 Boss 체력바 UI.
/// BossHealth.OnBossSpawned 이벤트에 자동 연결되고, 보스 사망 시 자동 숨김됩니다.
/// </summary>
public class BossHealthBar : MonoBehaviour
{
    [Header("참조 설정")]
    [Tooltip("보스 체력바 슬라이더 (Fill 방식)")] public Slider healthSlider;
    [Tooltip("체력 숫자를 표시할 TextMeshProUGUI (선택)")] public TextMeshProUGUI healthText;
    [Tooltip("체력바 루트 GameObject – 활성/비활성 전환용")] public GameObject rootObject;

    private BossHealth _boss;

    private void Awake()
    {
        // 초기에는 숨겨둡니다.
        if (rootObject != null) rootObject.SetActive(false);
        else if (healthSlider != null) healthSlider.gameObject.SetActive(false);

        // 보스 스폰 이벤트 등록
        BossHealth.OnBossSpawned += HandleBossSpawned;
    }

    private void OnDestroy()
    {
        BossHealth.OnBossSpawned -= HandleBossSpawned;
        UnsubscribeCurrent();
    }

    private void HandleBossSpawned(BossHealth bh)
    {
        // 이미 활성화된 보스를 무시 (하나만 표시)
        if (_boss != null) return;
        Subscribe(bh);
    }

    private void Subscribe(BossHealth bh)
    {
        _boss = bh;
        if (_boss == null) return;
        _boss.OnHealthChanged += OnBossHealthChanged;
        _boss.OnBossDead += OnBossDead;

        // UI 표시
        if (rootObject != null) rootObject.SetActive(true);
        else if (healthSlider != null) healthSlider.gameObject.SetActive(true);

        // 초기 값 설정
        OnBossHealthChanged(_boss.CurrentHealth, _boss.maxHealth);
    }

    private void UnsubscribeCurrent()
    {
        if (_boss != null)
        {
            _boss.OnHealthChanged -= OnBossHealthChanged;
            _boss.OnBossDead -= OnBossDead;
            _boss = null;
        }
    }

    private void OnBossHealthChanged(int cur, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = cur;
        }
        if (healthText != null)
        {
            healthText.text = $"{cur} / {max}";
        }
    }

    private void OnBossDead()
    {
        UnsubscribeCurrent();
        if (rootObject != null) rootObject.SetActive(false);
        else if (healthSlider != null) healthSlider.gameObject.SetActive(false);
    }
}
