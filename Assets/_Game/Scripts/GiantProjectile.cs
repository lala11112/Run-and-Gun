using UnityEngine;
using System.Collections;

/// <summary>
/// R 스킬로 발사되는 거대한 투사체.
/// 적에게 피해를 주고, 지나가는 경로상의 모든 적 투사체(EnemyProjectile, EnemyBullet 태그)를 삭제한다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class GiantProjectile : MonoBehaviour
{
    [Header("투사체 설정")]
    [Tooltip("이동 속도 (단위/초)")] public float speed = 15f;
    [Tooltip("데미지") ] public int damage = 3;
    [Tooltip("생존 시간(초)")] public float lifetime = 4f;

    [Header("추가 효과")]
    [Tooltip("적중 시 적을 기절시키는지 여부")] public bool stunOnHit = false;
    [Tooltip("기절 지속 시간")] public float stunDuration = 0.8f;
    [Tooltip("적중 시 적을 끌어당기는지 여부")] public bool pullOnHit = false;
    [Tooltip("끌어당기는 힘(Impulse)")] public float pullForce = 10f;

    [Header("곡선 궤도 설정")]
    [Tooltip("투사체가 곡선 궤도로 이동할지 여부")]
    public bool curved = false;

    [Tooltip("초당 회전 각도(도). 양수는 시계방향, 음수는 반시계방향")] public float curveDegreesPerSecond = 90f;

    [Header("충돌 처리")]
    [Tooltip("거대 투사체를 파괴할 환경 레이어")] public LayerMask environmentLayers;
    [Tooltip("환경 충돌 무시 시간(초) – 발사 직후 벽에 겹쳐도 바로 사라지지 않도록 함")] public float ignoreEnvironmentTime = 0.12f;

    private Vector2 _dir;
    private Rigidbody2D _rb;
    private float _spawnTime;
    private Coroutine _lifeRoutine;

    public void Init(Vector2 dir)
    {
        _dir = dir.normalized;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

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

            // 플레이어 투사체 간 충돌 무시
            Physics2D.IgnoreLayerCollision(projLayer, projLayer, true);
        }

        _lifeRoutine = StartCoroutine(ManageLifetime());
        _spawnTime = Time.time;
    }

    private IEnumerator ManageLifetime()
    {
        yield return new WaitForSeconds(lifetime);
        AdvancedObjectPool.Despawn(gameObject);
    }

    private void FixedUpdate()
    {
        if (_dir == Vector2.zero) return;

        // 곡선 궤도 적용: 방향 벡터를 매 FixedUpdate 마다 회전
        if (curved)
        {
            float angleRad = curveDegreesPerSecond * Mathf.Deg2Rad * Time.fixedDeltaTime;
            float cos = Mathf.Cos(angleRad);
            float sin = Mathf.Sin(angleRad);
            _dir = new Vector2(_dir.x * cos - _dir.y * sin, _dir.x * sin + _dir.y * cos).normalized;
        }

        _rb.linearVelocity = _dir * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 적 투사체 제거
        if (other.CompareTag("EnemyBullet"))
        {
            Destroy(other.gameObject);
            return; // 거대 투사체는 계속 진행
        }
        if (other.TryGetComponent(out EnemyProjectile ep))
        {
            Destroy(ep.gameObject);
            return;
        }

        // 환경 레이어 충돌 시 파괴 (기본: Wall, Ground 등 설정)
        if (((1 << other.gameObject.layer) & environmentLayers) != 0)
        {
            // 발사 직후 유예시간 내에는 벽과 충돌해도 무시
            if (Time.time - _spawnTime < ignoreEnvironmentTime)
            {
                return;
            }
            AdvancedObjectPool.Despawn(gameObject);
            return;
        }

        // 플레이어 또는 같은 투사체 레이어와의 충돌은 무시
        int playerLayer = LayerMask.NameToLayer("Player");
        int projLayer = LayerMask.NameToLayer("PlayerProjectile");
        if (other.gameObject.layer == playerLayer || other.gameObject.layer == projLayer)
        {
            return;
        }

        if (other.CompareTag("Player")) return;

        // 적에게 피해
        if (other.TryGetComponent(out SimpleEnemy enemy))
        {
            enemy.TakeDamage(damage);

            // 기절 효과
            if (stunOnHit)
            {
                enemy.Stun(stunDuration);
            }

            // 끌어당김 효과
            if (pullOnHit && enemy.TryGetComponent(out Rigidbody2D erb))
            {
                Vector2 dir = (Vector2)transform.position - erb.position;
                erb.AddForce(dir.normalized * pullForce, ForceMode2D.Impulse);
            }
        }

        else if (other.TryGetComponent(out IDamageable dmg))
        {
            dmg.TakeDamage(damage);
        }

        // 그 외 오브젝트와는 충돌해도 파괴되지 않음 (지속 진행)
    }
} 