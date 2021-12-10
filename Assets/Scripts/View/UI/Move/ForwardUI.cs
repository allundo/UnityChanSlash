using UnityEngine.EventSystems;
using UniRx;
using UniRx.Triggers;
using System;

public class ForwardUI : PointerEnterUI, IPointerDownHandler, IPointerUpHandler
{
    protected ForwardButton forwardButton;
    public IObservable<Unit> DashObservable { get; protected set; }

    protected override void InitObservable()
    {
        forwardButton = moveButton as ForwardButton;

        EnterObservable =
            Observable.Merge(forwardButton.IsPressed, forwardButton.IsDash)
                .Where(_ => forwardButton.IsPressed.Value && !forwardButton.IsDash.Value)
                .SelectMany(_ => moveButton.UpdateAsObservable())
                .TakeUntil(forwardButton.IsPressed.Where(x => !x))
                .RepeatUntilDestroy(forwardButton);

        DashObservable =
            forwardButton.IsDash.Where(x => x)
                .SelectMany(_ => forwardButton.UpdateAsObservable())
                .TakeUntil(forwardButton.IsDash.Where(x => !x))
                .RepeatUntilDestroy(forwardButton);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isActive) return;
        raycastMoveButton.ExecutePointerDown(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isActive) return;
        raycastMoveButton.ExecutePointerUp(eventData);
    }
}
