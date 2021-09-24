using UnityEngine.EventSystems;

public class PointerEnter : MoveButton, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isActive) PressButton();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ReleaseButton();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ExecuteEvents.Execute<IPointerEnterHandler>(gameObject, eventData, (handler, data) => handler.OnPointerEnter(data as PointerEventData));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ExecuteEvents.Execute<IPointerExitHandler>(gameObject, eventData, (handler, data) => handler.OnPointerExit(data as PointerEventData));
    }
}
