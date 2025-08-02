using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 여러 개의 StoryStepSO를 순서대로 담고 있는 컨테이너 ScriptableObject입니다.
/// 하나의 완성된 챕터 또는 시퀀스를 나타냅니다.
/// </summary>
[CreateAssetMenu(fileName = "NewStoryChapter", menuName = "Run and Gun/Story/Story Chapter")]
public class StoryChapterSO : ScriptableObject
{
    [Tooltip("순서대로 실행될 스토리 스텝들의 리스트")]
    public List<StoryStepSO> steps = new List<StoryStepSO>();
}
