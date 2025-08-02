using UnityEngine;

namespace Run_and_Gun.Story
{
    /// <summary>
    /// 지정된 이름의 씬을 로드하는 스토리 스텝입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "LoadSceneStep", menuName = "Run and Gun/Story/Steps/Load Scene")]
    public class LoadSceneStepSO : StoryStepSO
    {
        [Tooltip("로드할 씬의 이름입니다. 반드시 File > Build Settings에 해당 씬이 포함되어 있어야 합니다.")]
        public string sceneName;

        /// <summary>
        /// 이 스텝의 로직을 실행할 상태(State) 객체를 생성합니다.
        /// </summary>
        public override IStoryStepState CreateState(StoryPlayer storyPlayer)
        {
            return new LoadSceneState(this, storyPlayer);
        }
    }
}
