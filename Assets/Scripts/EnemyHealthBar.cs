using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 적 캐릭터 위에 체력바(Slider)를 표시하고, Enemy의 체력 변화에 따라 갱신.
/// Canvas 는 World Space 모드로 Enemy 하위에 두고 이 스크립트를 같은 오브젝트(또는 Canvas)에 부착.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class EnemyHealthBar : MonoBehaviour
{
    [Tooltip("체력 표시용 Slider")] public Slider healthSlider;
    [Tooltip("적 기준 Y 오프셋")] public Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);

    private Enemy _enemy;
    private Camera _mainCam;

    private void Awake()
    {
        _enemy = GetComponentInParent<Enemy>();
        _mainCam = Camera.main;
    }

    private void Start()
    {
        if (_enemy == null || healthSlider == null) { enabled = false; return; }

        healthSlider.maxValue = _enemy.maxHealth;
        healthSlider.value = _enemy.CurrentHealth;
        _enemy.OnHealthChanged += OnEnemyHealthChanged;
    }

    private void LateUpdate()
    {
        if (_enemy == null) return;
        // 위치를 Enemy 위로 고정
        transform.position = _enemy.transform.position + worldOffset;
        // 카메라를 향하도록(선택)
        if (_mainCam != null)
            transform.rotation = _mainCam.transform.rotation;
    }

    private void OnEnemyHealthChanged(int current, int max)
    {
        healthSlider.maxValue = max;
        healthSlider.value = Mathf.Clamp(current, 0, max);
    }

    private void OnDestroy()
    {
        if (_enemy != null)
            _enemy.OnHealthChanged -= OnEnemyHealthChanged;
    }
} 