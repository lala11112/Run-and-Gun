using UnityEngine;
using UnityEngine.SceneManagement;
using Com.LuisPedroFonseca.ProCamera2D;

/// <summary>
/// 카메라 관련 기능(흔들림, 시네마틱 등)을 총괄하는 싱글톤 매니저.
/// BootScene에서 생성되어 게임 내내 파괴되지 않고 유지됩니다.
/// </summary>
public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    /// <summary>
    /// 현재 씬의 ProCamera2DShake 컴포넌트 참조.
    /// </summary>
    private ProCamera2DShake _proCameraShake;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 씬이 로드될 때마다 새로운 카메라를 찾아 연결하기 위해 이벤트에 등록합니다.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // 오브젝트가 파괴될 때 이벤트 리스너를 정리합니다.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    /// <summary>
    /// 씬이 로드되었을 때 호출되는 콜백 함수입니다.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새로 로드된 씬에서 메인 카메라와 ProCamera2DShake 컴포넌트를 찾습니다.
        if (Camera.main != null)
        {
            _proCameraShake = Camera.main.GetComponent<ProCamera2DShake>();
            if (_proCameraShake == null)
            {
                // Gameplay 씬처럼 카메라 효과가 필요한 씬에서는 ProCamera2DShake가 있어야 합니다.
                // 모든 씬에 필요하지는 않으므로 Warning으로 처리합니다.
                Debug.LogWarning($"'{scene.name}' 씬의 메인 카메라에 ProCamera2DShake 컴포넌트가 없습니다.");
            }
        }
        else
        {
             Debug.LogWarning($"'{scene.name}' 씬에 메인 카메라가 없습니다.");
             _proCameraShake = null;
        }
    }

    /// <summary>
    /// 카메라 흔들림을 요청합니다.
    /// </summary>
    /// <param name="duration">지속 시간</param>
    /// <param name="magnitude">강도</param>
    [System.Obsolete("이제 프리셋을 사용해주세요. ShakeWithPreset(string presetName)를 사용하세요.")]
    public void Shake(float duration, float magnitude)
    {
        if (_proCameraShake != null)
        {
            // ProCamera2D의 Shake API를 직접 호출합니다.
            _proCameraShake.Shake(duration, new Vector2(magnitude, magnitude));
        }
        else
        {
            Debug.LogWarning("흔들림 효과를 재생하려 했으나, ProCamera2DShake 컴포넌트를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 이름으로 지정된 Shake 프리셋을 사용하여 카메라 흔들림을 요청합니다.
    /// </summary>
    /// <param name="presetName">ProCamera2DShake 컴포넌트에 설정된 프리셋 이름</param>
    public void ShakeWithPreset(string presetName)
    {
        if (string.IsNullOrEmpty(presetName)) return;

        if (_proCameraShake != null)
        {
            _proCameraShake.Shake(presetName);
        }
        else
        {
            Debug.LogWarning($"'{presetName}' 프리셋으로 흔들림 효과를 재생하려 했으나, ProCamera2DShake 컴포넌트를 찾을 수 없습니다.");
        }
    }
} 