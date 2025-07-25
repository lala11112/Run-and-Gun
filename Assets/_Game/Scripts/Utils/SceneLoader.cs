using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

/// <summary>
/// 간단한 비동기 씬 로더 – 로딩 화면 표시 등 확장 가능.
/// </summary>
public static class SceneLoader
{
    private static bool _isLoading;

    /// <summary>씬 로딩 시작 이벤트: string sceneName</summary>
    public static event Action<string> OnLoadStarted;
    /// <summary>씬 로딩 진행률 이벤트: float progress (0~1)</summary>
    public static event Action<float> OnLoadProgress;
    /// <summary>씬 로딩 완료 이벤트: string sceneName</summary>
    public static event Action<string> OnLoadCompleted;

    public static IEnumerator LoadSceneAsync(string sceneName)
    {
        if (_isLoading)
        {
            Debug.LogWarning("[SceneLoader] 이미 씬 로딩 중입니다.");
            yield break;
        }
        _isLoading = true;

        OnLoadStarted?.Invoke(sceneName);
        LoadingScreen.Show();

        // TODO: 로딩 화면 UI On

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
        {
            // TODO: 로딩 ProgressBar 업데이트 op.progress
            OnLoadProgress?.Invoke(op.progress);
            LoadingScreen.UpdateProgress(op.progress);
            yield return null;
        }

        OnLoadProgress?.Invoke(1f);
        OnLoadCompleted?.Invoke(sceneName);

        _isLoading = false;
        LoadingScreen.Hide();
        // TODO: 로딩 화면 UI Off (디자인 교체 가능)
    }
} 