using UnityEngine;

/// <summary>
/// 투사체(총알) 동작을 제어하는 스크립트
/// 직선 이동, 충돌 처리, 수명 관리를 담당
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("투사체 설정")]
    [Tooltip("투사체의 이동 속도 (단위/초)")]
    public float speed = 25f;
    
    [Tooltip("투사체가 자동으로 파괴되는 시간 (초)")]
    public float lifetime = 3f;
    
    [Tooltip("투사체가 적에게 가하는 피해량")]
    public int damage = 1;

    // 내부 변수
    private Vector2 _direction; // 투사체가 이동할 방향 벡터

    /// <summary>
    /// 투사체 초기화 메서드
    /// PlayerController에서 호출하여 방향을 설정
    /// </summary>
    /// <param name="direction">투사체가 이동할 방향 (정규화된 벡터)</param>
    public void Init(Vector2 direction)
    {
        // 방향 벡터를 정규화하여 저장
        _direction = direction.normalized;
        
        // 지정된 수명 후 자동으로 오브젝트 파괴
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// 매 프레임마다 투사체를 지정된 방향으로 이동
    /// </summary>
    private void Update()
    {
        // 설정된 방향과 속도로 투사체 이동
        // Vector2를 Vector3로 캐스팅하여 Transform.position에 적용
        transform.position += (Vector3)(_direction * speed * Time.deltaTime);
    }

    /// <summary>
    /// 2D 트리거 충돌 감지 메서드
    /// 다른 오브젝트와 충돌했을 때 호출됨
    /// </summary>
    /// <param name="other">충돌한 다른 오브젝트의 Collider2D</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어와의 충돌은 무시 (자기 자신이 발사한 투사체이므로)
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            return; // 충돌 처리하지 않고 종료
        }

        // 적 캐릭터 피해 처리
        if (other.TryGetComponent(out Enemy enemy))
        {
            enemy.TakeDamage(damage);
        }

        // 충돌 후 투사체 파괴
        Destroy(gameObject);
    }

    private void Awake()
    {
        // Ensure projectile is on PlayerProjectile layer
        int projLayer = LayerMask.NameToLayer("PlayerProjectile");
        if (projLayer != -1)
        {
            gameObject.layer = projLayer;

            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer != -1)
            {
                Physics2D.IgnoreLayerCollision(projLayer, playerLayer, true);
            }
        }

        _direction = _direction == Vector2.zero ? Vector2.up : _direction; // fallback safety
    }
} 