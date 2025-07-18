using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// C 스킬 사용 시 플레이어 대시 방향 전방에 생성되어 적 투사체를 막아내고 적에게 피해를 주는 임시 실드.
/// 일정 시간이 지나면 자동으로 파괴됩니다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Shield : MonoBehaviour
{
    [Tooltip("실행 중 코드에서 주입되는 수명(초)")] public float lifetime = 0.5f;

    // 플레이어(또는 타겟) Transform – CSkill에서 주입
    [HideInInspector] public Transform target;
    // 타겟 기준 오프셋 – CSkill에서 주입
    [HideInInspector] public Vector2 localOffset;
    // 실드가 적에게 줄 피해량 – CSkill에서 주입
    [HideInInspector] public int damage = 1;
    // 적 중첩 시 카메라 흔들림 – CSkill에서 주입
    [HideInInspector] public float shakeDuration = 0.08f;
    [HideInInspector] public float shakeMagnitude = 0.12f;

    // 이동 / 투사체 관련
    [HideInInspector] public bool followTarget = true; // true = 플레이어 따라다님, false = 투사체
    [HideInInspector] public Vector2 moveDir;          // followTarget=false 일 때 진행 방향(정규화)
    [HideInInspector] public float moveSpeed = 0f;     // 투사체 속도

    // 넉백 설정 (A 랭크 이상)
    [HideInInspector] public bool applyKnockback = false;
    [HideInInspector] public float knockbackForce = 6f;
    [HideInInspector] public float knockbackDistance = 1f;

    // 이미 피해를 준 적 캐싱
    private HashSet<int> _hitEnemies = new HashSet<int>();

    private void Awake()
    {
        // Collider2D를 트리거로 설정해 물리 충돌을 방지하고 투사체/적 감지만 수행
        if (TryGetComponent(out Collider2D col))
        {
            col.isTrigger = true;
        }

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (followTarget && target != null)
        {
            transform.position = (Vector2)target.position + localOffset;

            // 플레이어 기준 offset 방향으로 회전
            Vector2 dir = ((Vector2)transform.position - (Vector2)target.position);
            if (dir.sqrMagnitude > 0.0001f)
            {
                float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, ang - 90f);
            }
        }
        else if (!followTarget)
        {
            transform.position += (Vector3)(moveDir * moveSpeed * Time.deltaTime);

            // 이동 방향으로 회전 (2D: Z축 회전)
            if (moveDir.sqrMagnitude > 0.0001f)
            {
                float ang = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, ang - 90f);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 적에게 피해
        if (other.TryGetComponent(out Enemy enemy))
        {
            if (_hitEnemies.Contains(enemy.GetInstanceID())) return;
            _hitEnemies.Add(enemy.GetInstanceID());
            enemy.TakeDamage(damage);

            // 스타일 점수 등록
            StyleManager.Instance?.RegisterSkillHit(SkillType.C);

            // 카메라 흔들림
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(shakeDuration, shakeMagnitude);
            }

            // 넉백 적용
            if (applyKnockback)
            {
                Vector2 dir = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;
                if (enemy.TryGetComponent(out Rigidbody2D rb) && rb.bodyType != RigidbodyType2D.Kinematic)
                {
                    rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
                }
                else
                {
                    // Rigidbody가 Kinematic이거나 없을 경우 위치를 직접 이동시켜 밀어내는 느낌 부여
                    enemy.transform.position += (Vector3)(dir * knockbackDistance);
                }
            }
            return;
        }

        // 적 투사체 차단
        if (other.CompareTag("EnemyBullet"))
        {
            Destroy(other.gameObject);
            return;
        }

        if (other.TryGetComponent(out EnemyProjectile ep))
        {
            Destroy(ep.gameObject);
        }
    }
} 