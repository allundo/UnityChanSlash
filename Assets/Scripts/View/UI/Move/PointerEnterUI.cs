using UnityEngine.EventSystems;
using UniRx;
using UniRx.Triggers;
using System;

public class PointerEnterUI : MoveUI, IPointerEnterHandler, IPointerExitHandler
{
    public IObservable<Unit> EnterObservable { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        InitObservable();
    }

    protected virtual void InitObservable()
    {
        // To be observed every frame when IsPressed is true
        EnterObservable =
            moveButton.IsPressed
                .Where(x => x).SelectMany(_ => moveButton.FixedUpdateAsObservable()) // Update can be skipped and then cannot observe button off
                .TakeUntil(moveButton.IsPressed.Where(x => !x))
                .RepeatUntilDestroy(moveButton);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isActive) moveButton.PressButton();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        moveButton.ReleaseButton();
    }
}
