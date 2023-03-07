using UnityEngine;
using UnityEngine.EventSystems;

public class TestDrag : MonoBehaviour, IDragHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log($"Drag: {eventData.position}");
    }
    public void OnDestroy()
    {
        Debug.Log("Destroy");
    }
}
