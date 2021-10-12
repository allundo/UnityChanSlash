using UnityEngine.EventSystems;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

public class PointerEnterUI : MoveUI, IPointerEnterHandler, IPointerExitHandler
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
#if UNITY_EDITOR
        // BUG: Unintended pointer enter (0,0) is detected on Unity Editor.
        if (eventData.position == Vector2.zero) return;
#endif

        if (!isActive) return;
        Execute<IPointerEnterHandler>(eventData, (handler, data) => handler.OnPointerEnter(data as PointerEventData));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Execute<IPointerExitHandler>(eventData, (handler, data) => handler.OnPointerExit(data as PointerEventData));
    }
}
