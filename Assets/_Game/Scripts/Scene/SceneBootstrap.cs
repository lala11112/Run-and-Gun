using UnityEngine;

/// <summary>
/// 각 씬에 배치되어, 씬 특화 초기화를 수행합니다.
/// 예: Gameplay 씬에서 TestBattleManager를 활성화.
/// </summary>
public class SceneBootstrap : MonoBehaviour
{
    [Tooltip("Gameplay 씬에만 존재하는 TestBattleManager 프리팹")] public GameObject gameplayManagerPrefab;

    private void Start()
    {
        switch (GameManager.Instance.CurrentState)
        {
            case GameState.InGame:
                EnsureGameplayManager();
                break;
        }
    }

    private void EnsureGameplayManager()
    {
        if (FindObjectOfType<TestBattleManager>() == null && gameplayManagerPrefab != null)
        {
            Instantiate(gameplayManagerPrefab);
        }
    }
} 