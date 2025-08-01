using UnityEngine;
using PixelCrushers.DialogueSystem;

/// <summary>
/// '대화' 스텝의 데이터를 정의하는 ScriptableObject입니다.
/// Dialogue System for Unity 에셋과의 연동을 담당합니다.
/// </summary>
[CreateAssetMenu(fileName = "DialogueStep", menuName = "Story/Steps/Dialogue")]
public class DialogueStepSO : StoryStepSO
{
    [Header("대화 설정")]
    [Tooltip("Dialogue System에서 재생할 대화(Conversation)의 제목입니다.")]
    public string conversation;

    [Tooltip("대화의 액터로 지정할 플레이어의 Transform. 비워두면 'Player' 태그로 찾습니다.")]
    public Transform playerActor;

    [Tooltip("대화의 상대역으로 지정할 NPC의 Transform. 비워두면 'Conversant' 태그로 찾습니다.")]
    public Transform conversantActor;

    public override IStoryStepState CreateState(StoryPlayer storyPlayer)
    {
        return new DialogueState(this, storyPlayer);
    }
}

/// <summary>
/// DialogueStepSO의 데이터를 받아 실제 로직을 처리하는 상태 클래스입니다.
/// </summary>
public class DialogueState : IStoryStepState
{
    private readonly DialogueStepSO _data;
    private readonly StoryPlayer _storyPlayer;

    public DialogueState(DialogueStepSO data, StoryPlayer storyPlayer)
    {
        _data = data;
        _storyPlayer = storyPlayer;
    }

    public void Enter()
    {
        // DialogueManager.instance를 통해 실제 DialogueSystemController에 접근하여 이벤트를 구독합니다.
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.conversationEnded += OnConversationEnded;
        }
        
        Debug.Log($"[DialogueState] 대화 '{_data.conversation}'를 시작합니다.");
        // 대화를 시작합니다. 액터 정보를 함께 넘겨줍니다.
        DialogueManager.StartConversation(_data.conversation, _data.playerActor, _data.conversantActor);
    }

    private void OnConversationEnded(Transform conversant)
    {
        // 대화가 끝나면 다음 스텝으로 진행합니다.
        Debug.Log($"[DialogueState] 대화 '{_data.conversation}'가 종료되었습니다. 다음 스텝으로 진행합니다.");
        _storyPlayer.AdvanceToNextStep();
    }

    public void Tick() { }

    public void Exit()
    {
        // 마찬가지로 DialogueManager.instance를 통해 이벤트 구독을 해제합니다.
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.conversationEnded -= OnConversationEnded;
        }
    }
}
