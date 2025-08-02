using UnityEngine;

/// <summary>
/// 게임 전체에서 사용되는 설정값들을 관리하는 ScriptableObject입니다.
/// 하드코딩된 매직 넘버들을 이곳에서 중앙 관리합니다.
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "Run and Gun/Core/Game Config", order = 1)]
public class GameConfigSO : ScriptableObject
{
    [Header("플레이어 관련 설정")]
    [Tooltip("FirePoint 오브젝트의 이름입니다.")]
    public string firePointName = "FirePoint";
    
    [Tooltip("플레이어 태그 이름입니다.")]
    public string playerTagName = "Player";
    
    [Tooltip("기본 이동 속도 (PlayerStatsSO가 없을 때 사용)")]
    public float defaultMoveSpeed = 5f;

    [Header("전투 관련 설정")]
    [Tooltip("기본 넉백 힘입니다.")]
    public float defaultKnockbackForce = 10f;
    
    [Tooltip("적 탐지 최대 개수 (Physics 쿼리 배열 크기)")]
    public int maxEnemyDetection = 32;
    
    [Tooltip("타겟팅 최대 개수 (Z스킬 등에서 사용)")]
    public int maxTargetCount = 16;

    [Header("UI 관련 설정")]
    [Tooltip("적 총알 태그 이름입니다.")]
    public string enemyBulletTagName = "EnemyBullet";

    [Header("성능 관련 설정")]
    [Tooltip("오브젝트 풀 기본 크기입니다.")]
    public int defaultPoolSize = 20;
    
    [Tooltip("Physics 쿼리 결과 배열의 기본 크기입니다.")]
    public int physicsQueryArraySize = 32;
    
    [Header("Object Pool 설정")]
    [Tooltip("투사체 풀 크기입니다.")]
    public int projectilePoolSize = 50;
    
    [Tooltip("이펙트 풀 크기입니다.")]
    public int effectPoolSize = 30;
    
    [Tooltip("풀 사전 로딩 개수입니다. (게임 시작 시 미리 생성할 오브젝트 수)")]
    public int prewarmCount = 10;
}