using UnityEngine.EventSystems;
using UniRx;
using UniRx.Triggers;
using System;

public class PointerEnterUI : MoveUI, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public IObservable<Unit> EnterObservable { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        // To be observed every frame when IsPressed is true
        EnterObservable =
            moveButton.IsPressed
                .Where(x => x).SelectMany(_ => moveButton.UpdateAsObservable())
                .TakeUntil(moveButton.IsPressed.Where(x => !x))
                .RepeatUntilDestroy(moveButton);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Execute<IPointerEnterHandler>(eventData, (handler, data) => handler.OnPointerEnter(data as PointerEventData));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Execute<IPointerExitHandler>(eventData, (handler, data) => handler.OnPointerExit(data as PointerEventData));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Execute<IPointerEnterHandler>(eventData, (handler, data) => handler.OnPointerEnter(data as PointerEventData));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Execute<IPointerExitHandler>(eventData, (handler, data) => handler.OnPointerExit(data as PointerEventData));
    }
}
