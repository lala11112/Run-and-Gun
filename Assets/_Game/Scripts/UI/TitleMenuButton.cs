using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 타이틀 메뉴의 각 버튼에 부착하여 마우스 호버 이벤트를 GameEvents로 전달합니다.
/// </summary>
public class TitleMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("이 버튼이 담당할 상호작용 타입")]
    public TitleInteractionType interactionType;

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스가 버튼 위에 올라오면 해당 타입의 이벤트를 방송합니다.
        GameEvents.RaiseTitleInteractionHovered(interactionType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 마우스가 버튼을 벗어나면 '상호작용 없음' 이벤트를 방송합니다.
        GameEvents.RaiseTitleInteractionHovered(TitleInteractionType.None);
    }
} 