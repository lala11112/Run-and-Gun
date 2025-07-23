
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 적 캐릭터 위에 체력바(Slider)를 표시하고, 체력 변화에 따라 갱신.
/// Canvas 는 World Space 모드로 Enemy 하위에 두고 이 스크립트를 같은 오브젝트(또는 Canvas)에 부착.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class EnemyHealthBar : MonoBehaviour
{
    [Tooltip("체력 표시용 Slider")] public Slider healthSlider;
    [Tooltip("적 기준 Y 오프셋")] public Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);

    private LivingEntity _entity;
    private Camera _mainCam;

    private void Awake()
    {
        _entity = GetComponentInParent<LivingEntity>();
        _mainCam = Camera.main;
    }

    private void Start()
    {
        if (_entity == null || healthSlider == null) { enabled = false; return; }

        // 캔버스를 적 오브젝트의 자식으로 이동시켜 사망 시 함께 파괴되도록 함
        transform.SetParent(_entity.transform, true);

        healthSlider.maxValue = _entity.maxHealth;
        healthSlider.value    = _entity.CurrentHealth;
        _entity.OnHealthChanged += OnHealthChanged;
        // LivingEntity 사망 시 체력바 파괴
        _entity.OnHealthChanged += (cur, max)=>{ if (cur<=0) Destroy(gameObject); }; 
    }

    private void LateUpdate()
    {
        if (_entity == null) return;
        // 위치를 Enemy 위로 고정
        transform.position = _entity.transform.position + worldOffset;
        // 카메라를 향하도록(선택)
        if (_mainCam != null)
            transform.rotation = _mainCam.transform.rotation;
    }

    private void OnHealthChanged(int current, int max)
    {
        healthSlider.maxValue = max;
        healthSlider.value = Mathf.Clamp(current, 0, max);
    }

    private void OnDestroy()
    {
        if (_entity != null)
            _entity.OnHealthChanged -= OnHealthChanged;
    }
} 