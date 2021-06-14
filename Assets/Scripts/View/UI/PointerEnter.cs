using UnityEngine.EventSystems;

public class PointerEnter : MoveButton, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        PressButton();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ReleaseButton();
    }
}
