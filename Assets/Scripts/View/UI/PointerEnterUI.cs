using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PointerEnterUI : MoveUI, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Execute<IPointerEnterHandler>(eventData, (handler, data) => handler.OnPointerEnter(data as PointerEventData));
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        Execute<IPointerExitHandler>(eventData, (handler, data) => handler.OnPointerExit(data as PointerEventData));
    }
}