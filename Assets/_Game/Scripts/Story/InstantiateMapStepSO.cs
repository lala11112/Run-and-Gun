using UnityEngine;
using Unity.AI.Navigation;

/// <summary>
/// '맵 생성' 스텝의 데이터를 정의하는 ScriptableObject입니다.
/// </summary>
[CreateAssetMenu(fileName = "InstantiateMapStep", menuName = "Run and Gun/Story/Steps/Instantiate Map")]
public class InstantiateMapStepSO : StoryStepSO
{
    [Header("맵 설정")]
    [Tooltip("씬에 생성할 맵 프리팹입니다.")]
    public GameObject mapPrefab;

    [Tooltip("이 스텝 실행 시, 이전에 생성되었던 맵을 파괴할지 여부입니다.")]
    public bool clearPreviousMap = true;

    public override IStoryStepState CreateState(StoryPlayer storyPlayer)
    {
        return new InstantiateMapState(this, storyPlayer);
    }
}

/// <summary>
/// InstantiateMapStepSO의 데이터를 받아 실제 로직을 처리하는 상태 클래스입니다.
/// </summary>
public class InstantiateMapState : IStoryStepState
{
    private readonly InstantiateMapStepSO _data;
    private readonly StoryPlayer _storyPlayer;
    private GameObject _mapInstance;

    public InstantiateMapState(InstantiateMapStepSO data, StoryPlayer storyPlayer)
    {
        _data = data;
        _storyPlayer = storyPlayer;
    }

    public void Enter()
    {
        _storyPlayer.StartCoroutine(ProcessMap());
    }

    private System.Collections.IEnumerator ProcessMap()
    {
        // 1. 이전 맵 정리
        if (_data.clearPreviousMap && StoryPlayerContext.CurrentMap != null)
        {
            Object.Destroy(StoryPlayerContext.CurrentMap);
        }
        
        // 2. 새로운 맵 생성
        if (_data.mapPrefab != null)
        {
            _mapInstance = Object.Instantiate(_data.mapPrefab);
            StoryPlayerContext.CurrentMap = _mapInstance;

            // 3. NavMesh 리빌드
            var surface = _mapInstance.GetComponentInChildren<NavMeshSurface>();
            if (surface != null)
            {
                surface.BuildNavMesh();
                // NavMesh 빌드가 완료될 때까지 한 프레임 대기
                yield return null; 
            }
        }
        
        // 4. 모든 작업 완료 후 다음 스텝으로
        _storyPlayer.AdvanceToNextStep();
    }

    public void Tick() { }
    public void Exit() { }
}
