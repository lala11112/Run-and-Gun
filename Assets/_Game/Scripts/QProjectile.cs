using UnityEngine;
using System.Collections;

/// <summary>
/// 스킬에 의해 발사되는 '투사체'의 기본 행동을 정의합니다.
/// 데미지, 속도 등의 데이터는 발사하는 스킬로부터 주입받으며, 스스로는 이동과 소멸 로직만 담당합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class QProjectile : MonoBehaviour
{
    [Header("시각 효과 설정")]
    [Tooltip("투사체가 이동 방향을 향하도록 자동 회전할지 여부")] public bool faceMovingDirection = true;
    [Tooltip("트레일 렌더러가 유지될 시간(초)")] public float trailTime = 0.3f;
    [Tooltip("트레일 시작 두께")] public float trailStartWidth = 0.25f;
    [Tooltip("트레일 종료 두께")] public float trailEndWidth = 0f;
    [Tooltip("트레일 시작 색상")] public Color trailStartColor = new Color(1f, 0.95f, 0.4f, 1f);
    [Tooltip("트레일 종료 색상")] public Color trailEndColor = new Color(1f, 0.95f, 0.4f, 0f);
    [Tooltip("트레일에 사용할 머티리얼 (비우면 기본 Sprite-Default)")] public Material trailMaterial;

    private Rigidbody2D _rb;
    private Coroutine _lifeRoutine;
    
    // 스킬 로직에서 주입받는 데이터
    private Vector2 _direction;
    private float _speed;
    private int _damage;
    private float _lifetime;

    /// <summary>
    /// 투사체를 초기화하고 발사 준비를 합니다. ZSkillLogic과 같은 스킬 로직에서 호출해야 합니다.
    /// </summary>
    /// <param name="direction">이동 방향</param>
    /// <param name="damage">적에게 가할 피해량</param>
    /// <param name="speed">이동 속도</param>
    /// <param name="lifetime">생존 시간(초)</param>
    public void Init(Vector2 direction, int damage, float speed, float lifetime)
    {
        _direction = direction.normalized;
        _damage = damage;
        _speed = speed;
        _lifetime = lifetime;

        if (faceMovingDirection)
        {
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // Init이 호출된 후에 생존 시간 타이머를 시작합니다.
        if (_lifeRoutine != null)
        {
            StopCoroutine(_lifeRoutine);
        }
        _lifeRoutine = StartCoroutine(LifeRoutine());
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        SetupTrailRenderer();

        int projLayer = LayerMask.NameToLayer("PlayerProjectile");
        if (projLayer != -1)
        {
            gameObject.layer = projLayer;
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer != -1)
            {
                Physics2D.IgnoreLayerCollision(projLayer, playerLayer, true);
            }
            Physics2D.IgnoreLayerCollision(projLayer, projLayer, true);
        }
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = _direction * _speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out QTrailZone _)) return; // Q 장판과는 무시

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
        
        if (other.TryGetComponent(out IDamageable dmg) && !other.CompareTag("Player"))
        {
            dmg.TakeDamage(_damage);
            Despawn();
            return;
        }
        
        // 벽과 같은 IDamageable이 아닌 오브젝트와 충돌 시
        if (!other.isTrigger && !other.CompareTag("Player"))
        {
            Despawn();
        }
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
            new GradientColorKey[] { new GradientColorKey(trailStartColor, 0f), new GradientColorKey(trailEndColor, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(trailStartColor.a, 0f), new GradientAlphaKey(trailEndColor.a, 1f) }
        );
        tr.colorGradient = g;

        tr.minVertexDistance = 0.05f;
        tr.autodestruct = false;
        if (TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        {
            tr.sortingLayerID = spriteRenderer.sortingLayerID;
            tr.sortingOrder = spriteRenderer.sortingOrder - 1;
        }
    }

    private IEnumerator LifeRoutine()
    {
        yield return new WaitForSeconds(_lifetime);
        Despawn();
    }

    public void Despawn()
    {
        if (_lifeRoutine != null)
        {
            StopCoroutine(_lifeRoutine);
            _lifeRoutine = null;
        }
        
        // Object Pool 사용: Destroy 대신 풀로 반환
        AdvancedObjectPool.Despawn(gameObject);
    }
}