using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using PixelCrushers.DialogueSystem; // Dialogue System 네임스페이스 추가

/// <summary>
/// 테스트 전투를 관리하는 매니저.
/// - 시작 시 맵과 적을 랜덤으로 생성
/// - 모든 적이 제거되면 지정한 딜레이 후 맵을 교체하고 새 적을 스폰
/// - 반복적으로 무한 테스트 전투를 진행해 다양한 상황을 검증할 수 있도록 함
/// </summary>
public class TestBattleManager : MonoBehaviour
{
    [Header("맵/전투 설정")]
    [Tooltip("전투 시작 시와 라운드가 끝났을 때 랜덤으로 선택될 맵 프리팹 목록")]
    public GameObject[] mapPrefabs;

    [Tooltip("스폰될 적 프리팹 목록")]
    public GameObject[] enemyPrefabs;

    [Tooltip("라운드마다 소환될 적 수 (최소, 최대)")]
    public Vector2Int spawnCountRange = new Vector2Int(5, 10);

    [Tooltip("적이 스폰될 영역의 가로, 세로 크기 (NavMesh가 있는 범위여야 함)")]
    public Vector2 spawnAreaSize = new Vector2(20f, 20f);

    [Tooltip("라운드가 끝난 뒤 다음 라운드가 시작되기까지 대기 시간(초)")]
    public float nextRoundDelay = 3f;

    [Tooltip("씬에 존재하는 플레이어를 참조하거나, 비워두면 자동으로 Player 태그를 검색")] public Transform playerTransform;

    [Tooltip("라운드가 진행될 때마다 적 수 증가량")] public int spawnIncrementPerRound = 0;

    [Header("컷씬 대사 설정")]
    [Tooltip("3라운드 시작 시 자동 재생할 대화 이름 (Dialogue System)")] public string round3Conversation = "Round3_Cutscene";

    // 3라운드 대사가 이미 재생되었는지 확인하는 플래그
    private bool _round3DialoguePlayed = false;

    [Header("디버그")]
    [Tooltip("현재 라운드 수 (읽기 전용)")]
    public int currentRound = 0;

    private readonly List<Enemy> _aliveEnemies = new();
    private GameObject _currentMap;
    private bool _isRoundRunning;
    private Bounds _groundBounds;
    private bool _hasGroundBounds;

    private void Start()
    {
        StartCoroutine(RoundLoop());
    }

    /// <summary>
    /// 라운드를 순차적으로 진행하는 코루틴.
    /// </summary>
    private IEnumerator RoundLoop()
    {
        while (true)
        {
            yield return StartCoroutine(SetupRound());
            _isRoundRunning = true;

            // 라운드 종료 대기
            while (_isRoundRunning)
            {
                // 리스트에서 파괴된 Enemy 레퍼런스 제거
                _aliveEnemies.RemoveAll(e => e == null);
                if (_aliveEnemies.Count == 0)
                {
                    _isRoundRunning = false;
                }
                yield return null;
            }

            // 라운드 종료 처리
            yield return new WaitForSeconds(nextRoundDelay);
            CleanupCurrentRound();
        }
    }

    /// <summary>
    /// 맵을 생성하고 적을 스폰하여 새로운 라운드를 세팅.
    /// </summary>
    private IEnumerator SetupRound()
    {
        currentRound++;

        // 3라운드 컷씬 대사 재생
        if (currentRound == 3 && !_round3DialoguePlayed && !string.IsNullOrEmpty(round3Conversation))
        {
            _round3DialoguePlayed = true;
            // Dialogue System에 대화 시작 요청
            DialogueManager.StartConversation(round3Conversation);
            // 대사가 끝날 때까지 코루틴을 일시 대기 (전투 스폰 전)
            while (DialogueManager.IsConversationActive)
            {
                yield return null;
            }
        }

        // 1. 맵 생성 및 플레이어 위치 초기화
        SpawnRandomMap();
        RepositionPlayer();
        // NavMeshSurface2D를 사용할 경우, 프리팹 내부에 이미 베이크된 데이터가 있어 즉시 사용 가능.
        // 만약 베이크가 필요하면 여기서 Surface2D.BuildNavMesh() 호출 가능.

        // 2. 적 스폰
        int min = spawnCountRange.x + spawnIncrementPerRound * (currentRound - 1);
        int max = spawnCountRange.y + spawnIncrementPerRound * (currentRound - 1);
        int enemyCount = Random.Range(min, max + 1);
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnRandomEnemy();
            // 프레임 분산을 위해 한 프레임에 한 마리씩 스폰 (과도한 GC 방지)
            yield return null;
        }
    }

    /// <summary>
    /// 현재 라운드의 리소스를 정리.
    /// </summary>
    private void CleanupCurrentRound()
    {
        // 남아있는 적(만약 있을 경우) 제거
        foreach (var enemy in _aliveEnemies)
        {
            if (enemy != null) Destroy(enemy.gameObject);
        }
        _aliveEnemies.Clear();

        // 맵 제거
        if (_currentMap != null) Destroy(_currentMap);
    }

    /// <summary>
    /// 랜덤 맵 프리팹을 인스턴스화.
    /// </summary>
    private void SpawnRandomMap()
    {
        if (mapPrefabs == null || mapPrefabs.Length == 0)
        {
            Debug.LogWarning("[TestBattleManager] 맵 프리팹이 설정되지 않았습니다.");
            return;
        }
        int index = Random.Range(0, mapPrefabs.Length);
        _currentMap = Instantiate(mapPrefabs[index], transform.position, Quaternion.identity);
        CacheGroundBounds();
    }

    private void CacheGroundBounds()
    {
        _hasGroundBounds = false;

        if (_currentMap == null) return;

        // Ground 태그가 붙은 모든 Collider2D를 검색하여 경계 영역 계산
        var colliders = _currentMap.GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders)
        {
            if (!col.CompareTag("Ground")) continue;
            if (!_hasGroundBounds)
            {
                _groundBounds = col.bounds;
                _hasGroundBounds = true;
            }
            else
            {
                _groundBounds.Encapsulate(col.bounds);
            }
        }

        // Collider2D가 없을 수도 있으니 Renderer Bounds도 백업으로 시도
        if (!_hasGroundBounds)
        {
            var renders = _currentMap.GetComponentsInChildren<Renderer>();
            foreach (var rd in renders)
            {
                if (!rd.CompareTag("Ground")) continue;
                if (!_hasGroundBounds)
                {
                    _groundBounds = rd.bounds;
                    _hasGroundBounds = true;
                }
                else
                {
                    _groundBounds.Encapsulate(rd.bounds);
                }
            }
        }
    }

    /// <summary>
    /// NavMesh 상의 랜덤 위치에 적 프리팹을 소환.
    /// </summary>
    private void SpawnRandomEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("[TestBattleManager] 적 프리팹이 설정되지 않았습니다.");
            return;
        }

        // 1. 프리팹 선택
        int index = Random.Range(0, enemyPrefabs.Length);
        GameObject enemyPrefab = enemyPrefabs[index];

        // 2. 스폰 위치 결정 (NavMesh 샘플)
        Vector3 spawnPos = transform.position;
        const int maxAttempts = 10;
        bool found = false;

        for (int attempt = 0; attempt < maxAttempts && !found; attempt++)
        {
            Vector3 randomPos;
            if (_hasGroundBounds)
            {
                // Ground 영역 내 무작위 위치 선정
                randomPos = new Vector3(
                    Random.Range(_groundBounds.min.x, _groundBounds.max.x),
                    Random.Range(_groundBounds.min.y, _groundBounds.max.y),
                    0f);
            }
            else
            {
                // 기존 사각 범위 사용 (백업)
                randomPos = transform.position + new Vector3(
                    Random.Range(-spawnAreaSize.x * 0.5f, spawnAreaSize.x * 0.5f),
                    Random.Range(-spawnAreaSize.y * 0.5f, spawnAreaSize.y * 0.5f),
                    0f);
            }

            if (NavMesh.SamplePosition(randomPos, out var hit, 1f, NavMesh.AllAreas))
            {
                spawnPos = hit.position;
                found = true;
            }
        }

        if (!found)
        {
            Debug.LogWarning("[TestBattleManager] NavMesh 내에서 유효한 스폰 위치를 찾지 못했습니다. (기본 원점 사용)");
        }

        // 3. 적 생성 및 목록에 추가
        GameObject go = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        Enemy enemy = go.GetComponent<Enemy>();
        if (enemy != null) _aliveEnemies.Add(enemy);
        else Debug.LogWarning("[TestBattleManager] 적 프리팹에 Enemy 컴포넌트가 없습니다.");
    }

    private void RepositionPlayer()
    {
        if (playerTransform == null)
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }
        if (playerTransform == null) return;

        // 플레이어가 맵 바깥으로 튕겨져 나가는 현상을 방지하기 위해,
        // 맵이 갱신될 때마다 플레이어를 월드 원점(0,0)으로 강제 이동시킨다.
        // 필요하다면 여기서 Z(또는 Y) 레이어 값을 조정할 수 있다.
        Vector3 targetPos = Vector3.zero;

        // NavMeshAgent로 이동 중일 수 있으므로 Warp 사용
        var agent = playerTransform.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.Warp(targetPos);
        }
        else
        {
            playerTransform.position = targetPos;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 에디터에서 스폰 영역 시각화
        Gizmos.color = Color.yellow;
        if (_hasGroundBounds)
        {
            Gizmos.DrawWireCube(_groundBounds.center, _groundBounds.size);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0f));
        }
    }
#endif
}