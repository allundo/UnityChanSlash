using UnityEngine.EventSystems;

public class PointerEnter : MoveButton, IPointerEnterHandler, IPointerExitHandler
{
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (isActive) PressButton();
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        ReleaseButton();
    }
}
