using UnityEngine;

/// <summary>
/// 적 투사체 – 이동 방향으로 회전하고 트레일 잔상을 남깁니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("투사체 설정")]
    [Tooltip("투사체 속도 (단위/초)")]
    public float speed = 8f;

    [Tooltip("투사체가 플레이어에게 가하는 피해량")]
    public int damage = 1;

    [Tooltip("투사체가 이동 방향을 향하도록 자동 회전할지 여부")] public bool faceMovingDirection = true;

    [Header("트레일 렌더러 설정")]
    [Tooltip("트레일 유지 시간(초)")] public float trailTime = 0.35f;
    [Tooltip("트레일 시작 두께")] public float trailStartWidth = 0.2f;
    [Tooltip("트레일 종료 두께")] public float trailEndWidth = 0f;
    [Tooltip("트레일 시작 색상")] public Color trailStartColor = new Color(1f,0.4f,0.4f,1f);
    [Tooltip("트레일 종료 색상")] public Color trailEndColor = new Color(1f,0.4f,0.4f,0f);
    [Tooltip("트레일에 사용할 머티리얼 (비우면 기본 Sprite-Default)")] public Material trailMaterial;

    [Tooltip("투사체가 자동으로 파괴되는 시간 (초)")]
    public float lifetime = 5f;

    private Vector2 _direction = Vector2.zero;
    private Rigidbody2D _rb;

    /// <summary>
    /// 발사체 이동 방향 지정 (정규화된 벡터)
    /// </summary>
    public void Init(Vector2 dir)
    {
        _direction = dir.normalized;

        if (faceMovingDirection)
        {
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        SetupTrailRenderer();
        // 수명 타이머 설정
        Destroy(gameObject, lifetime);
    }

    private void SetupTrailRenderer()
    {
        if (TryGetComponent(out TrailRenderer _)) return;
        TrailRenderer tr = gameObject.AddComponent<TrailRenderer>();
        tr.time = trailTime;
        tr.startWidth = trailStartWidth;
        tr.endWidth = trailEndWidth;
        tr.material = trailMaterial != null ? trailMaterial : new Material(Shader.Find("Sprites/Default"));

        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[]{ new GradientColorKey(trailStartColor,0f), new GradientColorKey(trailEndColor,1f)},
            new GradientAlphaKey[]{ new GradientAlphaKey(trailStartColor.a,0f), new GradientAlphaKey(trailEndColor.a,1f)});
        tr.colorGradient = g;

        tr.minVertexDistance = 0.05f;
        tr.autodestruct = false;
        int layerID = GetComponent<SpriteRenderer>()?.sortingLayerID ?? 0;
        int order = GetComponent<SpriteRenderer>()?.sortingOrder ?? 0;
        tr.sortingLayerID = layerID;
        tr.sortingOrder = order-1;
    }

    private void FixedUpdate()
    {
        // Rigidbody2D 속도 갱신 (FixedUpdate에서 수행)
        if (_direction == Vector2.zero) return;
        _rb.linearVelocity = _direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 동일한 적 또는 다른 투사체와 충돌 시 무시
        if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet")) return;

        // 플레이어와 충돌 체크 (루트에 PlayerHealth 존재 여부)
        if (other.TryGetComponent(out PlayerHealth ph))
        {
            ph.TakeDamage(damage);
        }

        // 충돌 후 파괴
        Destroy(gameObject);
    }
} 