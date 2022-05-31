using UnityEngine.EventSystems;
using UniRx;

public class PointerEnterExitUI : MoveUI, IPointerEnterHandler, IPointerExitHandler
{
    public IReadOnlyReactiveProperty<bool> IsPressed => moveButton.IsPressed;


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isActive) moveButton.PressButton();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        moveButton.ReleaseButton();
    }
}
