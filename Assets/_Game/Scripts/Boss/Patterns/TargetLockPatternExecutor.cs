using System.Collections;
using UnityEngine;

/// <summary>
/// Target Lock 패턴의 실행기입니다.
/// BossPatternSO와 함께 사용되어 유도탄 공격을 수행합니다.
/// </summary>
public class TargetLockPatternExecutor : MonoBehaviour, IBossPatternExecutor
{
    [Header("패턴 설정")]
    [Tooltip("유도탄 프리팹")]
    public GameObject missilePrefab;
    
    [Tooltip("연발 횟수")]
    public int missileCount = 4;
    
    [Tooltip("발사 간격")]
    public float missileInterval = 0.6f;
    
    [Tooltip("미사일 속도")]
    public float missileSpeed = 5f;
    
    [Tooltip("미사일 데미지")]
    public int missileDamage = 10;

    public IEnumerator Execute(BossPatternSO patternData, BossController bossController)
    {
        Debug.Log($"[TargetLockPattern] {patternData.patternName} 시작");
        
        // 패턴 데이터에서 설정 오버라이드 (필요시)
        float actualDuration = patternData.duration;
        
        Transform player = bossController.FindPlayer();
        if (player == null)
        {
            Debug.LogWarning("[TargetLockPattern] 플레이어를 찾을 수 없습니다.");
            yield break;
        }

        // 미사일 발사
        for (int i = 0; i < missileCount; i++)
        {
            if (bossController.IsDead) break;
            
            // 플레이어 위치 다시 확인 (이동했을 수 있음)
            player = bossController.FindPlayer();
            if (player == null) break;

            FireMissile(bossController, player);
            
            // 마지막 미사일이 아니면 대기
            if (i < missileCount - 1)
            {
                yield return new WaitForSeconds(missileInterval);
            }
        }

        Debug.Log($"[TargetLockPattern] {patternData.patternName} 완료");
    }

    private void FireMissile(BossController bossController, Transform target)
    {
        if (missilePrefab == null) return;

        Vector3 direction = bossController.GetDirectionToPlayer();
        Vector3 spawnPosition = bossController.transform.position;
        
        // 오브젝트 풀에서 미사일 생성
        GameObject missile = AdvancedObjectPool.Spawn(missilePrefab, spawnPosition, Quaternion.identity);
        
        // 미사일 초기화
        if (missile.TryGetComponent(out EnemyProjectile projectile))
        {
            projectile.Init(direction);
            projectile.speed = missileSpeed;
            projectile.damage = missileDamage;
        }
        
        // 유도탄 로직 (필요시 별도 컴포넌트로 구현)
        if (missile.TryGetComponent(out HomingMissile homing))
        {
            homing.SetTarget(target);
        }

        // 사운드 효과
        PlayMissileSound(bossController);
    }

    private void PlayMissileSound(BossController bossController)
    {
        // MasterAudio를 사용한 사운드 재생
        try
        {
            DarkTonic.MasterAudio.MasterAudio.PlaySound3DAtTransform("BossMissile", bossController.transform);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[TargetLockPattern] 사운드 재생 실패: {e.Message}");
        }
    }
}

/// <summary>
/// 유도탄 로직을 담당하는 컴포넌트 (예시)
/// </summary>
public class HomingMissile : MonoBehaviour
{
    [Header("유도 설정")]
    [Tooltip("유도 강도")]
    public float homingStrength = 2f;
    
    [Tooltip("최대 유도 시간")]
    public float maxHomingTime = 3f;

    private Transform _target;
    private float _homingTimer;
    private EnemyProjectile _projectile;

    private void Awake()
    {
        _projectile = GetComponent<EnemyProjectile>();
        _homingTimer = maxHomingTime;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    private void Update()
    {
        if (_target == null || _projectile == null) return;
        
        _homingTimer -= Time.deltaTime;
        if (_homingTimer <= 0f) return; // 유도 시간 종료

        // 타겟 방향으로 서서히 방향 조정
        Vector3 targetDirection = (_target.position - transform.position).normalized;
        Vector3 currentDirection = _projectile.direction;
        
        Vector3 newDirection = Vector3.Slerp(currentDirection, targetDirection, homingStrength * Time.deltaTime);
        _projectile.direction = newDirection.normalized;
        
        // 회전도 조정
        float angle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}