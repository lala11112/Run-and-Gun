using UnityEngine;

/// <summary>
/// 실드 드론의 정면 실드 충돌 판정 담당.
/// - 정면 각도 내에서 플레이어 공격이 닿으면 <see cref="ShieldDrone.OnShieldHit"/> 호출.
/// - 플레이어가 C 스킬 대시 상태일 때 실드에 닿으면 <see cref="ShieldDrone.BreakShield"/> 호출.
/// 본 스크립트는 실드 GameObject(Trigger Collider)에 부착합니다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ShieldCollider : MonoBehaviour
{
    private ShieldDrone _drone;
    private Collider2D _col;

    private void Awake()
    {
        _col = GetComponent<Collider2D>();
        _col.isTrigger = true;
        _drone = GetComponentInParent<ShieldDrone>();
        if (_drone == null)
        {
            Debug.LogError("ShieldCollider: 상위에 ShieldDrone 컴포넌트가 필요합니다.", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_drone == null) return;

        // 플레이어 대시(C 스킬)로 실드 파괴 체크
        if (other.TryGetComponent(out PlayerController pc) && pc.IsDashing)
        {
            _drone.BreakShield();
            return;
        }

        // 투사체/스킬 충돌에 대한 정면 각도 체크
        Vector2 toOther = (Vector2)(other.transform.position - _drone.transform.position);
        Vector2 shieldForward = ((Vector2)transform.position - (Vector2)_drone.transform.position).normalized; // 드론 기준 실드 전방
        float angle = Vector2.Angle(shieldForward, toOther);
        if (angle <= _drone.shieldHalfAngle)
        {
            // 정면에서 맞음 → 실드 반응
            _drone.OnShieldHit();

            // 정면 공격 무효화 – 투사체라면 삭제
            if (other.gameObject.layer == LayerMask.NameToLayer("PlayerProjectile"))
            {
                Destroy(other.gameObject);
            }
        }
        else
        {
            // 측후면 – 실드 관통, 아무 처리 안 함 (Enemy 가 피해 처리)
        }
    }
} 