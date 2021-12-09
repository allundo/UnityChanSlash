using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

public class RaycastHandler
{
    private MaskableGraphic image;

    public RaycastHandler(MaskableGraphic image)
    {
        this.image = image;
    }
    public RaycastHandler(GameObject gameObject) : this(gameObject.GetComponent<MaskableGraphic>()) { }

    public void RaycastPointerDown(PointerEventData eventData) => RaycastEvent(eventData, eventPointerDown);
    public void RaycastPointerUp(PointerEventData eventData) => RaycastEvent(eventData, eventPointerUp);
    public void RaycastPointerEnter(PointerEventData eventData) => RaycastEvent(eventData, eventPointerEnter);
    public void RaycastPointerExit(PointerEventData eventData) => RaycastEvent(eventData, eventPointerExit);
    public void RaycastDrag(PointerEventData eventData) => RaycastEvent(eventData, eventDrag);

    private ExecuteEvents.EventFunction<IPointerDownHandler> eventPointerDown = (handler, data) => handler.OnPointerDown(data as PointerEventData);
    private ExecuteEvents.EventFunction<IPointerUpHandler> eventPointerUp = (handler, data) => handler.OnPointerUp(data as PointerEventData);
    private ExecuteEvents.EventFunction<IPointerEnterHandler> eventPointerEnter = (handler, data) => handler.OnPointerEnter(data as PointerEventData);
    private ExecuteEvents.EventFunction<IPointerExitHandler> eventPointerExit = (handler, data) => handler.OnPointerExit(data as PointerEventData);
    private ExecuteEvents.EventFunction<IDragHandler> eventDrag = (handler, data) => handler.OnDrag(data as PointerEventData);

    private void RaycastEvent<T>(PointerEventData eventData, ExecuteEvents.EventFunction<T> eventFunc) where T : IEventSystemHandler
    {
        var objectsHit = new List<RaycastResult>();

        // Exclude this UI object from raycast target
        image.raycastTarget = false;

        EventSystem.current.RaycastAll(eventData, objectsHit);

        image.raycastTarget = true;

        foreach (var objectHit in objectsHit)
        {
            if (!ExecuteEvents.CanHandleEvent<T>(objectHit.gameObject))
            {
                continue;
            }

            ExecuteEvents.Execute<T>(objectHit.gameObject, eventData, eventFunc);
            break;
        }
    }
}
