using UnityEngine;

/// <summary>
/// E 대시 중·직후 플레이어 주변을 공전하는 탄막.
/// 스타일 랭크에 따라 크기, 투사체 반사, 적 끌어당김 등이 달라진다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DashOrbitBullet : MonoBehaviour
{
    // Runtime-injected parameters (세팅은 PlayerSkills에서 할당)
    [HideInInspector] public Transform center;      // 회전 중심(플레이어)
    [HideInInspector] public float radius = 1f;     // 궤도 반경
    [HideInInspector] public float angularSpeedDeg = 360f; // 각속도(˚/s)
    [HideInInspector] public int damage = 3;        // 기본 데미지
    [HideInInspector] public float lifetime = 0.6f; // 유지 시간
    [HideInInspector] public float startAngleDeg;   // 초기 각도

    [Header("랭크 효과")]
    [Tooltip("B 랭크 크기 배수")] public float sizeBMultiplier = 1.4f;
    [Tooltip("A 랭크 크기 배수")] public float sizeAMultiplier = 1.8f;
    [Tooltip("A 랭크 적 끌어당기는 힘")] public float pullForce = 6f;
    [Tooltip("A 랭크 투사체 반사 속도")] public float reflectSpeed = 14f;

    private float _angleRad;
    private StyleRank _rank;

    private void Awake()
    {
        // 트리거 콜라이더
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        Destroy(gameObject, lifetime);
    }

    private void Start()
    {
        _angleRad = startAngleDeg * Mathf.Deg2Rad;
        _rank = StyleManager.Instance != null ? StyleManager.Instance.CurrentRank : StyleRank.C;

        // 랭크별 크기 조정
        float scaleMul = 1f;
        if (_rank == StyleRank.B) scaleMul = sizeBMultiplier;
        else if (_rank == StyleRank.A) scaleMul = sizeAMultiplier;
        transform.localScale *= scaleMul;

        // CircleCollider2D 반경도 스케일에 맞춰 확장
        if (TryGetComponent(out CircleCollider2D cc))
        {
            cc.radius *= scaleMul;
        }

        // 초기 위치 설정
        if (center != null)
        {
            Vector2 offset = new Vector2(Mathf.Cos(_angleRad), Mathf.Sin(_angleRad)) * radius;
            transform.position = (Vector2)center.position + offset;
        }
    }

    private void Update()
    {
        if (center == null) { Destroy(gameObject); return; }

        _angleRad += angularSpeedDeg * Mathf.Deg2Rad * Time.deltaTime;
        Vector2 offset = new Vector2(Mathf.Cos(_angleRad), Mathf.Sin(_angleRad)) * radius;
        transform.position = (Vector2)center.position + offset;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 적 피해 + 끌어당김
        if (other.TryGetComponent(out Enemy enemy))
        {
            enemy.TakeDamage(damage);

            if (_rank == StyleRank.A && enemy.TryGetComponent(out Rigidbody2D erb))
            {
                Vector2 dir = (Vector2)center.position - erb.position;
                erb.AddForce(dir.normalized * pullForce, ForceMode2D.Impulse);
            }

            StyleManager.Instance?.RegisterSkillHit(SkillType.C);
        }

        // A 랭크 투사체 반사
        if (_rank == StyleRank.A)
        {
            if (other.CompareTag("EnemyBullet"))
            {
                ReflectProjectile(other.attachedRigidbody, other.transform.position);
                return;
            }
            if (other.TryGetComponent(out EnemyProjectile ep))
            {
                ReflectProjectile(ep.GetComponent<Rigidbody2D>(), ep.transform.position);
                return;
            }
        }
    }

    private void ReflectProjectile(Rigidbody2D projRb, Vector3 projPos)
    {
        if (projRb == null) { Destroy(projRb?.gameObject); return; }

        Vector2 dir = (projPos - center.position).normalized;
        projRb.linearVelocity = dir * reflectSpeed;
    }
} 