using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 간단한 비동기 씬 로더 – 로딩 화면 표시 등 확장 가능.
/// </summary>
public static class SceneLoader
{
    private static bool _isLoading;

    public static IEnumerator LoadSceneAsync(string sceneName)
    {
        if (_isLoading)
        {
            Debug.LogWarning("[SceneLoader] 이미 씬 로딩 중입니다.");
            yield break;
        }
        _isLoading = true;

        // TODO: 로딩 화면 UI On

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
        {
            // TODO: 로딩 ProgressBar 업데이트 op.progress
            yield return null;
        }

        _isLoading = false;
        // TODO: 로딩 화면 UI Off
    }
} 