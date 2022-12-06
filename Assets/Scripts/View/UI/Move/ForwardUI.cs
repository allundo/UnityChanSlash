using UnityEngine.EventSystems;
using UniRx;
using UniRx.Triggers;
using System;

public class ForwardUI : PointerEnterUI, IPointerDownHandler, IPointerUpHandler
{
    protected ForwardButton forwardButton;

    public bool IsActive => forwardButton.isActive;

    public void SetDashInputActive(bool isForwardingOrTrigger)
    {
        forwardButton.isDashInputActive = isForwardingOrTrigger;
    }

    public IObservable<bool> DashObservable => forwardButton.IsDash.SkipLatestValueOnSubscribe().Where(isDash => isDash);

    protected override void InitObservable()
    {
        forwardButton = moveButton as ForwardButton;

        EnterObservable =
            Observable.Merge(forwardButton.IsPressed, forwardButton.IsDash)
                .Where(_ => forwardButton.IsPressed.Value && !forwardButton.IsDash.Value)
                .SelectMany(_ => moveButton.UpdateAsObservable())
                .TakeUntil(forwardButton.IsPressed.Where(x => !x))
                .RepeatUntilDestroy(forwardButton);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isActive) forwardButton.PointerDown();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isActive) forwardButton.PointerUp();
    }
}
