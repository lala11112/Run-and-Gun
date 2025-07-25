using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
/// <summary>
/// 개발 빌드 전용 디버그 단축키 – F10 으로 즉시 게임오버(Result 씬) 확인.
/// </summary>
public class DebugHotkeys : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DebugForceGameOver(false);
                Debug.Log("[Debug] Forced GameOver (F10)");
            }
        }
    }
}
#endif 