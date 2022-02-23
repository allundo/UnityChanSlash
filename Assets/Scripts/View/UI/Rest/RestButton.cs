using UniRx;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class RestButton : Button, IPointerDownHandler, IPointerUpHandler
{
    private RaycastHandler raycastHandler;
    private RectTransform rectTransform;

    private ISubject<Unit> clickSubject = new Subject<Unit>();
    public IObservable<Unit> Click => clickSubject;

    protected override void Awake()
    {
        base.Awake();
        raycastHandler = new RaycastHandler(gameObject);
        rectTransform = GetComponent<RectTransform>();
    }

    public void ResetOrientation(DeviceOrientation orientation)
    {
        switch (orientation)
        {
            case DeviceOrientation.Portrait:
                rectTransform.anchoredPosition = -rectTransform.sizeDelta * 0.5f - new Vector2(20f, 20f + Screen.height * ThirdPersonCamera.Margin);
                break;

            case DeviceOrientation.LandscapeRight:

                rectTransform.anchoredPosition = -rectTransform.sizeDelta * 0.5f - new Vector2(20f, 10f);
                break;
        }
    }

#if UNITY_EDITOR
    // BUG: Input Action "Position[Pointer]" causes pointer event double firing on Editor.
    private InputControl ic = new InputControl();
#endif

    public override void OnPointerDown(PointerEventData eventData)
    {
#if UNITY_EDITOR
        // BUG: Input Action "Position[Pointer]" causes pointer event double firing on Editor.
        if (!ic.CanFire()) return;
#endif
        if (interactable)
        {
            base.OnPointerDown(eventData);
            return;
        }

        raycastHandler.RaycastPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
#if UNITY_EDITOR
        // BUG: Input Action "Position[Pointer]" causes pointer event double firing on Editor.
        if (!ic.CanFire()) return;
#endif
        if (interactable)
        {
            base.OnPointerUp(eventData);
            clickSubject.OnNext(Unit.Default);
            return;
        }

        raycastHandler.RaycastPointerUp(eventData);
    }
}