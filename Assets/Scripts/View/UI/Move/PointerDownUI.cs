using UnityEngine.EventSystems;
using UniRx;
using System;

public class PointerDownUI : MoveUI, IPointerDownHandler
{
    public IObservable<Unit> PressObservable => (moveButton as PointerDown).PressSubject;

    public void OnPointerDown(PointerEventData eventData)
    {
        moveButton.ReleaseButton();
    }
}
