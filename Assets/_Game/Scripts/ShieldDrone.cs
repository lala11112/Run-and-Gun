using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// 배리어형 적 "실드 드론".
/// 정면 실드로 공격을 막고, 실드 피격 시 3연속 돌진으로 반격합니다.
/// C 스킬 등 특정 충격에 실드가 파괴(Break)되면 일정 시간 무방비 상태.
/// </summary>
[RequireComponent(typeof(Enemy))]
[RequireComponent(typeof(Rigidbody2D))]
public class ShieldDrone : MonoBehaviour
{
    private enum State { Idle, Follow, ShieldReact, Dash1, Dash2, Dash3, Cooldown, Break }

    [Header("실드 설정")]
    [Tooltip("실드가 방어할 각도 ±도수")] public float shieldHalfAngle = 30f;
    [Tooltip("실드 회전 속도 도/초")] public float shieldTurnSpeed = 60f;
    [Tooltip("실드 GameObject (Collider 포함)")] public Transform shieldTransform;
    [Tooltip("실드 Break 상태 지속 시간")] public float breakDuration = 2.5f;

    [Header("실드 위치")]
    [Tooltip("드론 중심으로부터 실드까지의 거리")] public float shieldOffset = 0.8f;

    [Header("돌진 설정")]
    [Tooltip("각 돌진별 거리")] public float dashDistance = 2.5f;
    [Tooltip("돌진 속도 단계 1,2,3")] public float[] dashSpeed = {2f,3f,4f};
    [Tooltip("돌진 준비 경고 시간")] public float dashPrepTime = 0.3f;
    [Tooltip("돌진 간 딜레이(단계별)")] public float[] dashDelays = {0.2f,0.1f};
    [Tooltip("충돌 시 플레이어에게 주는 데미지")] public int dashDamage = 1;

    [Header("텔레그래프 설정")]
    [Tooltip("돌진 경로를 미리 표시할 LineRenderer 프리팹")] public LineRenderer telegraphPrefab;
    [Tooltip("텔레그래프 컬러")] public Color telegraphColor = Color.red;
    [Tooltip("텔레그래프 두께")] public float telegraphWidth = 0.05f;

    [Header("반격 쿨타임")]
    [Tooltip("실드 반격(돌진) 사용 후 다시 사용할 때까지 대기 시간(초)")] public float reactCooldown = 3f;

    private float _cooldownTimer = 0f;

    private State _state = State.Idle;
    private Enemy _enemy;
    private NavMeshAgent _agent;
    private Rigidbody2D _rb;
    private Transform _player;
    private EnemyNavFollower _follower;
    private readonly List<GameObject> _activeTelegraphs = new();
    private bool _shieldActive = true;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _agent = GetComponent<NavMeshAgent>();
        if (_agent != null) { _agent.updateUpAxis=false; _agent.updateRotation=false; }
        _rb = GetComponent<Rigidbody2D>();
        _player = GameObject.FindWithTag("Player")?.transform;

        _follower = GetComponent<EnemyNavFollower>();
    }

    private void Update()
    {
        if (_player == null) return;

        if(_cooldownTimer>0f) _cooldownTimer-=Time.deltaTime;

        switch (_state)
        {
            default:
                break;
        }

        // 실드 회전 (항상 플레이어 향함)
        if (shieldTransform != null)
        {
            Vector3 toPlayer = _player.position - transform.position;
            Vector2 dirNorm = ((Vector2)toPlayer).normalized;

            // 실드 위치: 드론 전방(플레이어 방향)으로 shieldOffset 만큼 이동
            shieldTransform.position = transform.position + (Vector3)(dirNorm * shieldOffset);

            // 실드가 항상 플레이어를 향하도록 회전
            float targetAngle = Mathf.Atan2(dirNorm.y, dirNorm.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRot = Quaternion.Euler(0f,0f,targetAngle);
            shieldTransform.rotation = Quaternion.RotateTowards(shieldTransform.rotation, targetRot, shieldTurnSpeed * Time.deltaTime);
        }
    }

    private void SwitchState(State next)
    {
        StopAllCoroutines();
        _state = next;
        if (_agent!=null) _agent.isStopped = (next!=State.Idle);

        if (next == State.ShieldReact)
            StartCoroutine(ShieldReactRoutine());
        else if (next==State.Break)
            StartCoroutine(BreakRoutine());
    }

    /// <summary>
    /// 실드에 정면에서 피격될 때 호출 (ShieldCollider에서)
    /// </summary>
    public void OnShieldHit()
    {
        if (!_shieldActive) return;
        if(_cooldownTimer>0f) return; // 쿨타임 중엔 반응 X

        if (_state==State.Idle)
        {
            _cooldownTimer = reactCooldown; // 쿨타임 시작
            SwitchState(State.ShieldReact);
        }
    }

    /// <summary>
    /// 실드가 C 스킬 등으로 파괴되면 호출
    /// </summary>
    public void BreakShield()
    {
        if(!_shieldActive) return;
        _shieldActive=false;
        // TODO: 시각 효과 – 실드 OFF
        SwitchState(State.Break);
    }

    private IEnumerator BreakRoutine()
    {
        float timer = breakDuration;
        while(timer>0f)
        {
            timer-=Time.deltaTime;
            yield return null;
        }
        _shieldActive=true;
        SwitchState(State.Follow);
    }

    private IEnumerator ShieldReactRoutine()
    {
        // 준비 단계에서 완전히 정지 및 NavFollower 비활성
        if (_rb != null) _rb.linearVelocity = Vector2.zero;
        if (_agent != null) _agent.isStopped = true;
        if (_follower != null) _follower.enabled = false;

        // 3연속 돌진
        for(int i=0;i<3;i++)
        {
			// 먼저 완전히 정지시킨 뒤 방향 계산 (미끄러짐 방지)
			if (_rb != null) _rb.linearVelocity = Vector2.zero;
			if (_agent != null) _agent.isStopped = true;
			if (_follower != null) _follower.enabled = false;
			// 준비 경고 – 경로 표시
			Vector2 dir = (_player.position-transform.position).normalized;
            Vector3 lockPos = transform.position;

            LineRenderer lr = null;
            if (telegraphPrefab != null)
            {
                lr = Instantiate(telegraphPrefab, Vector3.zero, Quaternion.identity);
                _activeTelegraphs.Add(lr.gameObject);
            }
            else
            {
                GameObject go = new GameObject("DashTelegraph");
                lr = go.AddComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = lr.endColor = telegraphColor;
                lr.startWidth = lr.endWidth = telegraphWidth;
                lr.positionCount = 2;
            }

            Vector3 startPos = transform.position;

            // 드론 반지름만큼 추가해 실제 돌진 종료 지점과 일치시키기
            float radius = 0f;
            if (TryGetComponent(out Collider2D selfCol)) radius = selfCol.bounds.extents.magnitude;
            Vector3 endPos = startPos + (Vector3)(dir * (dashDistance + radius));
            lr.positionCount = 2;
            lr.SetPosition(0, startPos);
            lr.SetPosition(1, endPos);

            float prepTimer = 0f;
            while (prepTimer < dashPrepTime)
            {
                prepTimer += Time.deltaTime;
                if (_rb != null) _rb.linearVelocity = Vector2.zero; // 완전 고정
                transform.position = lockPos; // 위치 고정
                yield return null;
            }

            if (lr != null)
            {
                _activeTelegraphs.Remove(lr.gameObject);
                Destroy(lr.gameObject);
            }
            // 실제 돌진 (방향 고정)
            // dir 이미 계산됨
            float speed = dashSpeed[Mathf.Clamp(i,0,dashSpeed.Length-1)];
            float dist = dashDistance;
            float traveled=0f;
            _rb.linearVelocity = dir*speed;
            while(traveled<dist)
            {
                float step = speed*Time.deltaTime;
                traveled+=step;
                yield return null;
            }
            _rb.linearVelocity=Vector2.zero;
            if(i<dashDelays.Length) yield return new WaitForSeconds(dashDelays[i]);
        }
        // 돌진 일괄 종료 후 쿨다운으로
        if (_follower != null) _follower.enabled = true;
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        _state = State.Cooldown;
        if(_agent!=null) _agent.isStopped=true;
        yield return new WaitForSeconds(2.5f);
        _state = State.Idle;
        if(_agent!=null) _agent.isStopped=false;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if((_state==State.Dash1||_state==State.Dash2||_state==State.Dash3||_state==State.ShieldReact))
        {
            if(col.collider.CompareTag("Player"))
            {
                if(col.collider.TryGetComponent(out PlayerHealth ph))
                {
                    if (StyleManager.Instance != null && StyleManager.Instance.CurrentRank == StyleRank.S)
                        return;   // S 랭크 상태에서는 데미지, 연출 모두 무시
                    ph.TakeDamage(dashDamage);
                }
            }
        }
    }

    private void OnDestroy()
    {
        // 보스/드론 파괴 시 남아있는 텔레그래프 정리
        foreach (var go in _activeTelegraphs)
        {
            if (go != null) Destroy(go);
        }
        _activeTelegraphs.Clear();
    }
} 