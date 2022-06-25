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

    public bool ExecutePointerDown(Vector2 screenPos) => ExecutePointerDown(SimplePointerEventData(screenPos));
    public bool ExecutePointerUp(Vector2 screenPos) => ExecutePointerUp(SimplePointerEventData(screenPos));

    public bool RaycastPointerDown(Vector2 screenPos, bool includeTarget = false)
        => RaycastPointerDown(SimplePointerEventData(screenPos), includeTarget);
    public bool RaycastPointerUp(Vector2 screenPos, bool includeTarget = false)
        => RaycastPointerUp(SimplePointerEventData(screenPos), includeTarget);

    private PointerEventData SimplePointerEventData(Vector2 screenPos)
    {
        var eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPos;
        return eventData;
    }

    public bool ExecutePointerDown(PointerEventData eventData) => Execute(target, eventData, eventPointerDown);
    public bool ExecutePointerUp(PointerEventData eventData) => Execute(target, eventData, eventPointerUp);
    public bool ExecutePointerEnter(PointerEventData eventData) => Execute(target, eventData, eventPointerEnter);
    public bool ExecutePointerExit(PointerEventData eventData) => Execute(target, eventData, eventPointerExit);
    public bool ExecuteDrag(PointerEventData eventData) => Execute(target, eventData, eventDrag);

    public bool RaycastPointerDown(PointerEventData eventData, bool includeTarget = false) => RaycastEvent(eventData, eventPointerDown, includeTarget);
    public bool RaycastPointerUp(PointerEventData eventData, bool includeTarget = false) => RaycastEvent(eventData, eventPointerUp, includeTarget);
    public bool RaycastPointerEnter(PointerEventData eventData, bool includeTarget = false) => RaycastEvent(eventData, eventPointerEnter, includeTarget);
    public bool RaycastPointerExit(PointerEventData eventData, bool includeTarget = false) => RaycastEvent(eventData, eventPointerExit, includeTarget);
    public bool RaycastDrag(PointerEventData eventData, bool includeTarget = false) => RaycastEvent(eventData, eventDrag, includeTarget);

    private EventFunction<IPointerDownHandler> eventPointerDown = (handler, data) => handler.OnPointerDown(data as PointerEventData);
    private EventFunction<IPointerUpHandler> eventPointerUp = (handler, data) => handler.OnPointerUp(data as PointerEventData);
    private EventFunction<IPointerEnterHandler> eventPointerEnter = (handler, data) => handler.OnPointerEnter(data as PointerEventData);
    private EventFunction<IPointerExitHandler> eventPointerExit = (handler, data) => handler.OnPointerExit(data as PointerEventData);
    private EventFunction<IDragHandler> eventDrag = (handler, data) => handler.OnDrag(data as PointerEventData);

    private bool RaycastEvent<T>(PointerEventData eventData, EventFunction<T> eventFunc, bool includeTarget = false) where T : IEventSystemHandler
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

            return Execute<T>(objectHit.gameObject, eventData, eventFunc);
        }

        return false;
    }
}
