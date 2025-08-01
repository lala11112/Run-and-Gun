using System.Collections;
using UnityEngine;

namespace Run_and_Gun.Story
{
    /// <summary>
    /// LoadSceneStepSO의 로직을 실제로 수행하는 상태 클래스입니다.
    /// </summary>
    public class LoadSceneState : IStoryStepState
    {
        private readonly LoadSceneStepSO _step;
        private readonly StoryPlayer _storyPlayer;

        public LoadSceneState(LoadSceneStepSO step, StoryPlayer storyPlayer)
        {
            _step = step;
            _storyPlayer = storyPlayer;
        }

        public void Enter()
        {
            // sceneName이 비어있는 경우 오류를 출력하고 즉시 다음 스텝으로 진행합니다.
            if (string.IsNullOrEmpty(_step.sceneName))
            {
                Debug.LogError("로드할 씬 이름이 지정되지 않았습니다!");
                _storyPlayer.AdvanceToNextStep(); // 정확한 메서드 이름으로 수정
                return;
            }

            // SceneLoader를 통해 씬을 비동기적으로 로드하는 코루틴을 시작합니다.
            _storyPlayer.StartCoroutine(LoadScene());
        }

        private IEnumerator LoadScene()
        {
            // SceneLoader의 이벤트에 콜백 함수를 등록하여 로드 완료 시점을 감지합니다.
            SceneLoader.OnLoadCompleted += HandleSceneLoaded;
            
            // SceneLoader.LoadSceneAsync 코루틴을 실행합니다.
            // 이 코루틴이 끝날 때까지 기다릴 필요는 없습니다. 
            // HandleSceneLoaded 콜백이 호출되면서 다음 스텝으로 진행되기 때문입니다.
            yield return _storyPlayer.StartCoroutine(SceneLoader.LoadSceneAsync(_step.sceneName));
        }

        /// <summary>
        /// 씬 로드가 완료되었을 때 SceneLoader로부터 호출되는 콜백 함수입니다.
        /// </summary>
        /// <param name="sceneName">로드 완료된 씬의 이름입니다.</param>
        private void HandleSceneLoaded(string sceneName)
        {
            // 다른 콜백에 영향을 주지 않도록 이벤트에서 자신을 즉시 제거합니다.
            SceneLoader.OnLoadCompleted -= HandleSceneLoaded;
            
            // 씬 전환이 완료되었으므로, 스토리 플레이어에게 다음 스텝으로 진행하라고 알립니다.
            // 로드한 씬이 우리가 의도한 씬일 때만 다음 스텝으로 진행하는 방어 코드를 추가합니다.
            if (sceneName == _step.sceneName)
            {
                _storyPlayer.AdvanceToNextStep(); // 정확한 메서드 이름으로 수정
            }
        }

        public void Tick()
        {
            // 씬 로딩 중에는 매 프레임마다 특별히 처리할 작업이 없습니다.
        }

        public void Exit()
        {
            // 이 상태를 빠져나갈 때 등록했던 이벤트 핸들러가 혹시 남아있다면 안전하게 제거합니다.
            SceneLoader.OnLoadCompleted -= HandleSceneLoaded;
        }
    }
}
