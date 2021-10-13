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

    public void RaycastEvent<T>(PointerEventData eventData, ExecuteEvents.EventFunction<T> eventFunc) where T : IEventSystemHandler
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
