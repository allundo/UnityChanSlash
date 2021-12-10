using UnityEngine.EventSystems;
using UnityEngine;
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
        raycastMoveButton.ExecutePointerEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        raycastMoveButton.ExecutePointerExit(eventData);
    }
}
