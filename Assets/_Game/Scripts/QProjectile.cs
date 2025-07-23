using UnityEngine;
using System.Collections;

/// <summary>
/// Q 스킬로 발사되는 전용 투사체.
/// 적에게 피해를 주고, 적 투사체(EnemyProjectile, EnemyBullet 태그)를 삭제합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class QProjectile : MonoBehaviour
{
    [Header("투사체 설정")]
    [Tooltip("투사체 속도 (단위/초)")]
    public float speed = 18f;
    [Tooltip("투사체 피해량")]
    public int damage = 1;
    [Tooltip("생존 시간 (초)")]
    public float lifetime = 3f;
    [Tooltip("투사체가 이동 방향을 향하도록 자동 회전할지 여부")] public bool faceMovingDirection = true;

    [Header("트레일 렌더러 설정")]
    [Tooltip("트레일 렌더러가 유지될 시간(초)")] public float trailTime = 0.3f;
    [Tooltip("트레일 시작 두께")] public float trailStartWidth = 0.25f;
    [Tooltip("트레일 종료 두께")] public float trailEndWidth = 0f;
    [Tooltip("트레일 시작 색상")] public Color trailStartColor = new Color(1f, 0.95f, 0.4f, 1f); // 밝은 노랑
    [Tooltip("트레일 종료 색상")] public Color trailEndColor = new Color(1f, 0.95f, 0.4f, 0f); // 투명 노랑
    [Tooltip("트레일에 사용할 머티리얼 (비우면 기본 Sprite-Default)")] public Material trailMaterial;

    private Vector2 _dir;
    private Rigidbody2D _rb;
    private Coroutine _lifeRoutine;

    public void Init(Vector2 dir)
    {
        _dir = dir.normalized;

        // ---------- 이동 방향으로 회전 ----------
        if (faceMovingDirection)
        {
            float angle = Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        // ---------------------------------------
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        // ---------- 트레일 렌더러 동적 생성 ----------
        SetupTrailRenderer();
        // ------------------------------------------------

        // 레이어 설정 및 플레이어 충돌 무시
        int projLayer = LayerMask.NameToLayer("PlayerProjectile");
        if (projLayer != -1)
        {
            gameObject.layer = projLayer;
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer != -1)
            {
                Physics2D.IgnoreLayerCollision(projLayer, playerLayer, true);
            }

            // Q 스킬 투사체끼리 충돌 무시 (PlayerProjectile ↔ PlayerProjectile)
            Physics2D.IgnoreLayerCollision(projLayer, projLayer, true);
        }
    }

    private void OnEnable()
    {
        _lifeRoutine = StartCoroutine(LifeRoutine());
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = _dir * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out QTrailZone _))
        {
            return; // Q 장판과는 무시
        }
        // EnemyProjectile 또는 EnemyBullet 태그 제거
        if (other.CompareTag("EnemyBullet"))
        {
            Destroy(other.gameObject);
            Despawn();
            return;
        }
        if (other.TryGetComponent(out EnemyProjectile ep))
        {
            Destroy(ep.gameObject);
            Despawn();
            return;
        }

        if (other.TryGetComponent(out SimpleEnemy enemy))
        {
            enemy.TakeDamage(damage);
            if (StyleManager.Instance != null && StyleManager.Instance.CurrentRank == StyleRank.A)
            {
                enemy.Stun(0.5f);
            }
            StyleManager.Instance?.RegisterSkillHit(SkillType.Z);
            Despawn();
            return;
        }

        // 기타 IDamageable
        if (other.CompareTag("Player"))
        {
            Despawn();
            return;
        }

        else if (other.TryGetComponent(out IDamageable dmg))
        {
            dmg.TakeDamage(damage);
            StyleManager.Instance?.RegisterSkillHit(SkillType.Z);
            Despawn();
            return;
        }

        // 벽 등
        Despawn();
    }

    /// <summary>
    /// 투사체 뒤에 잔상을 남기는 트레일 렌더러를 동적으로 생성하고 설정합니다.
    /// </summary>
    private void SetupTrailRenderer()
    {
        // 이미 존재한다면 중복 생성 방지
        if (TryGetComponent(out TrailRenderer _)) return;

        TrailRenderer tr = gameObject.AddComponent<TrailRenderer>();
        tr.time = trailTime;
        tr.startWidth = trailStartWidth;
        tr.endWidth = trailEndWidth;
        tr.material = trailMaterial != null ? trailMaterial : new Material(Shader.Find("Sprites/Default"));

        // 색상 그라디언트 설정
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(trailStartColor, 0f),
                new GradientColorKey(trailEndColor, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(trailStartColor.a, 0f),
                new GradientAlphaKey(trailEndColor.a, 1f)
            }
        );
        tr.colorGradient = g;

        tr.minVertexDistance = 0.05f; // 부드러운 곡선
        tr.autodestruct = false;      // 투사체 파괴 시 자동 제거됨
        tr.sortingLayerID = gameObject.GetComponent<SpriteRenderer>()?.sortingLayerID ?? 0;
        tr.sortingOrder = (gameObject.GetComponent<SpriteRenderer>()?.sortingOrder ?? 0) - 1; // 스프라이트 뒤에 배치
    }

    private IEnumerator LifeRoutine()
    {
        yield return new WaitForSeconds(lifetime);
        Despawn();
    }

    public void Despawn()
    {
        if (_lifeRoutine != null)
        {
            StopCoroutine(_lifeRoutine);
            _lifeRoutine = null;
        }
        SimplePool.Despawn(gameObject);
    }
} 