using System.Collections;
using UnityEngine;

/// <summary>
/// BootState – 게임 실행 직후 데이터 로드 및 타이틀 씬으로 전환을 담당합니다.
/// </summary>
public class BootState : IState
{
    private readonly StateMachine _sm;
    private readonly GameManager _gm;

    public BootState(StateMachine sm, GameManager gm)
    {
        _sm = sm;
        _gm = gm;
    }

    public void Enter()
    {
        Debug.Log("[BootState] Enter – 타이틀 씬 로딩 시작");
        _gm.StartCoroutine(BootSequence());
    }

    public void Exit()
    {
        Debug.Log("[BootState] Exit");
    }

    public void Tick() { /* 부트 상태에서는 별도 갱신 로직이 필요 없음 */ }

    private IEnumerator BootSequence()
    {
        // TODO: 세이브/옵션 로드 등 부트 초기화 로직 추가
        SaveService.Initialize();
        if (CurrencyService.Instance == null)
        {
            new GameObject("CurrencyService").AddComponent<CurrencyService>();
        }
        if (SettingsManager.Instance == null)
        {
            new GameObject("SettingsManager").AddComponent<SettingsManager>();
        }
        if (CameraManager.Instance == null)
        {
            new GameObject("CameraManager").AddComponent<CameraManager>();
        }
        ApplyOptions();
        yield return _gm.StartCoroutine(SceneLoader.LoadSceneAsync(_gm.titleSceneName));
        _sm.ChangeState(new TitleState(_sm, _gm));
    }

    private void ApplyOptions()
    {
        // 예시: 오디오 볼륨 적용 (AudioMixer 별도 구현 필요)
        float vol = SaveService.Data.masterVolume;
        AudioListener.volume = vol;
    }
} 