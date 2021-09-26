using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using UniRx;
using DG.Tweening;

public class BaseHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] protected HandleUI handleRUI = default;
    [SerializeField] protected HandleUI handleLUI = default;
    [SerializeField] protected FlickInteraction flickR = default;
    [SerializeField] protected FlickInteraction flickL = default;
    [SerializeField] protected float maxAlpha = 0.8f;

    protected IObservable<Unit> ObserveUp => Observable.Merge(flickR.UpSubject, flickL.UpSubject);
    protected IObservable<Unit> ObserveDown => Observable.Merge(flickR.DownSubject, flickL.DownSubject);
    protected IObservable<Unit> ObserveRL => Observable.Merge(flickR.LeftSubject, flickL.RightSubject);

    protected RawImage rawImage;

    protected float alpha = 0.0f;
    protected bool isActive = false;
    private bool isFingerDown = false;

    protected HandleUI[] handleUIs;
    private FlickInteraction currentFlick = null;
    private Vector2 pressPos = Vector2.zero;

    public bool IsPressed => currentFlick != null;

    private Vector2 DragVector(Vector2 screenPos) => screenPos - pressPos;

    protected virtual void Awake()
    {
        rawImage = GetComponent<RawImage>();

        handleUIs = new[] { handleRUI, handleLUI };
    }

    protected virtual void Start()
    {
        ResetCenterPos();

        gameObject.SetActive(false);
        InactivateButtons();
    }

    protected virtual void Update()
    {
        UpdateTransparent();
    }

    private void UpdateTransparent()
    {
        alpha += (isActive ? 3f : -6f) * Time.deltaTime;

        if (alpha > maxAlpha)
        {
            alpha = maxAlpha;
            return;
        }

        if (alpha < 0.0f)
        {
            alpha = 0.0f;

            gameObject.SetActive(false);
            InactivateButtons();

            return;
        }
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (!isFingerDown) return;

        isFingerDown = false;

        if (currentFlick == null)
        {
            RaycastEvent<IPointerUpHandler>(eventData, (handler, data) => handler.OnPointerUp(data as PointerEventData));
            return;
        }

        if (!isActive) return;

        if (currentFlick == flickL) handleRUI.Activate();
        if (currentFlick == flickR) handleLUI.Activate();

        currentFlick.Release(DragVector(eventData.position));
        currentFlick = null;
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        isFingerDown = true;

        FlickInteraction flick = GetFlick(eventData.position);

        if (flick == null)
        {
            RaycastEvent<IPointerDownHandler>(eventData, (handler, data) => handler.OnPointerDown(data as PointerEventData));
            return;
        }

        if (!isActive) return;

        pressPos = eventData.position;
        currentFlick = flick;

        if (currentFlick == flickL) handleRUI.Inactivate();
        if (currentFlick == flickR) handleLUI.Inactivate();

        currentFlick.UpdateImage(DragVector(eventData.position));
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (!isFingerDown) return;

        if (currentFlick == null)
        {
            RaycastEvent<IDragHandler>(eventData, (handler, data) => handler.OnDrag(data as PointerEventData));
            return;
        }

        currentFlick.UpdateImage(DragVector(eventData.position));
    }

    public void ResetCenterPos()
    {
        handleUIs.ForEach(handleUI => handleUI.ResetCenterPos());
    }

    protected virtual FlickInteraction GetFlick(Vector2 screenPos)
    {
        if (handleRUI.InRegion(screenPos)) return flickR;
        if (handleLUI.InRegion(screenPos)) return flickL;

        return null;
    }

    public void Activate()
    {
        if (isActive) return;

        isActive = true;
        gameObject.SetActive(true);

        ActivateButtons();
    }

    protected virtual void ActivateButtons()
    {
        alpha = 0.0f;

        handleRUI.Activate();
        handleLUI.Activate();
    }

    private void InactivateButtons()
    {
        handleUIs.ForEach(handleUI => handleUI.Inactivate());
        currentFlick?.FadeOut()?.Play();
    }

    public void Inactivate()
    {
        if (!isActive) return;

        ButtonCancel(true);
        isActive = false;
    }

    protected void ButtonCancel(bool isFadeOnly = false)
    {
        if (isFadeOnly)
        {
            currentFlick?.FadeOut()?.Play();
        }
        else
        {
            currentFlick?.Cancel();
        }

        isFingerDown = false;
        currentFlick = null;
        pressPos = Vector2.zero;
    }

    public virtual void SetActive(bool value)
    {
        if (value)
        {
            Activate();
        }
        else
        {
            Inactivate();
        }
    }

    protected void RaycastEvent<T>(PointerEventData eventData, ExecuteEvents.EventFunction<T> eventFunc) where T : IEventSystemHandler
    {
        var objectsHit = new List<RaycastResult>();

        // Exclude this UI object from raycast target
        rawImage.raycastTarget = false;

        EventSystem.current.RaycastAll(eventData, objectsHit);

        rawImage.raycastTarget = true;

        foreach (var objectHit in objectsHit)
        {
            if (!ExecuteEvents.CanHandleEvent<T>(objectHit.gameObject))
            {
                continue;
            }

            ExecuteEvents.Execute<T>(objectHit.gameObject, eventData, eventFunc);
            break;
        }
    }
}
