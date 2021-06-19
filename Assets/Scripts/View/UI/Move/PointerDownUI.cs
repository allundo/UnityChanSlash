using UnityEngine.EventSystems;
using UniRx;
using System;

public class PointerDownUI : MoveUI, IPointerDownHandler
{
    public IObservable<Unit> PressObservable => (moveButton as PointerDown).PressSubject;

    public void OnPointerDown(PointerEventData eventData)
    {
        Execute<IPointerDownHandler>(eventData, (handler, data) => handler.OnPointerDown(data as PointerEventData));
    }
}