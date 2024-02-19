using UnityEngine.EventSystems;
using UniRx;
using UniRx.Triggers;
using System;

public class ForwardUI : PointerEnterUI, IPointerDownHandler, IPointerUpHandler
{
    protected ForwardButton forwardButton;
    private bool isPointerEnter = false;
    private bool isPointerDown = false;

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
                .SelectMany(_ => moveButton.FixedUpdateAsObservable()) // Update can be skipped and then cannot observe button off
                .TakeUntil(forwardButton.IsPressed.Where(x => !x))
                .RepeatUntilDestroy(forwardButton);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        if (isActive) forwardButton.PointerDown();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPointerDown) return;

        if (isActive) forwardButton.PointerUp();

        isPointerDown = false;

        // Make sure to call OnPointerExit
        if (isPointerEnter) OnPointerExit(eventData);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        isPointerEnter = true;
        if (isActive) moveButton.PressButton();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (!isPointerEnter) return;

        // Make sure to call OnPointerUp
        if (isPointerDown) OnPointerUp(eventData);

        moveButton.ReleaseButton();

        isPointerEnter = false;
    }
}
