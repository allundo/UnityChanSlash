using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PointerDownUI : MoveUI, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        Execute<IPointerDownHandler>(eventData, (handler, data) => handler.OnPointerDown(data as PointerEventData));
    }
}