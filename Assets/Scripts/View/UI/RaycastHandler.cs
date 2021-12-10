using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using static UnityEngine.EventSystems.ExecuteEvents;

public class RaycastHandler
{
    private MaskableGraphic image;
    private GameObject target;

    public RaycastHandler(MaskableGraphic image)
    {
        this.image = image;
        target = image.gameObject;
    }

    public RaycastHandler(GameObject gameObject) : this(gameObject.GetComponent<MaskableGraphic>()) { }

    public void ExecutePointerDown(PointerEventData eventData) => Execute(target, eventData, eventPointerDown);
    public void ExecutePointerUp(PointerEventData eventData) => Execute(target, eventData, eventPointerUp);
    public void ExecutePointerEnter(PointerEventData eventData) => Execute(target, eventData, eventPointerEnter);
    public void ExecutePointerExit(PointerEventData eventData) => Execute(target, eventData, eventPointerExit);
    public void ExecuteDrag(PointerEventData eventData) => Execute(target, eventData, eventDrag);

    public void RaycastPointerDown(PointerEventData eventData, bool includeTarget = false) => RaycastEvent(eventData, eventPointerDown, includeTarget);
    public void RaycastPointerUp(PointerEventData eventData, bool includeTarget = false) => RaycastEvent(eventData, eventPointerUp, includeTarget);
    public void RaycastPointerEnter(PointerEventData eventData, bool includeTarget = false) => RaycastEvent(eventData, eventPointerEnter, includeTarget);
    public void RaycastPointerExit(PointerEventData eventData, bool includeTarget = false) => RaycastEvent(eventData, eventPointerExit, includeTarget);
    public void RaycastDrag(PointerEventData eventData, bool includeTarget = false) => RaycastEvent(eventData, eventDrag, includeTarget);

    private EventFunction<IPointerDownHandler> eventPointerDown = (handler, data) => handler.OnPointerDown(data as PointerEventData);
    private EventFunction<IPointerUpHandler> eventPointerUp = (handler, data) => handler.OnPointerUp(data as PointerEventData);
    private EventFunction<IPointerEnterHandler> eventPointerEnter = (handler, data) => handler.OnPointerEnter(data as PointerEventData);
    private EventFunction<IPointerExitHandler> eventPointerExit = (handler, data) => handler.OnPointerExit(data as PointerEventData);
    private EventFunction<IDragHandler> eventDrag = (handler, data) => handler.OnDrag(data as PointerEventData);

    private void RaycastEvent<T>(PointerEventData eventData, EventFunction<T> eventFunc, bool includeTarget = false) where T : IEventSystemHandler
    {
        var objectsHit = new List<RaycastResult>();

        // Exclude this UI object from raycast target if includeTarget is FALSE
        image.raycastTarget = includeTarget;

        EventSystem.current.RaycastAll(eventData, objectsHit);

        image.raycastTarget = true;

        foreach (var objectHit in objectsHit)
        {
            if (!CanHandleEvent<T>(objectHit.gameObject))
            {
                continue;
            }

            Execute<T>(objectHit.gameObject, eventData, eventFunc);
            break;
        }
    }
}
