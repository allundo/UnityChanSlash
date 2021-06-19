using UnityEngine.EventSystems;
using UnityEngine;

public class PointerEnter : MoveButton, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        PressButton();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ReleaseButton();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ExecuteEvents.Execute<IPointerEnterHandler>(gameObject, eventData, (handler, data) => handler.OnPointerEnter(data as PointerEventData));
    }
}
