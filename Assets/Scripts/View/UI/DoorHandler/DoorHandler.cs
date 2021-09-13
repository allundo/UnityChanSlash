using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using UniRx;

public class DoorHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private DoorUI openDoorUI = default;
    [SerializeField] private DoorUI closeRDoorUI = default;
    [SerializeField] private DoorUI closeLDoorUI = default;
    [SerializeField] private DoorFlick openFlick = default;
    [SerializeField] private DoorFlick closeRFlick = default;
    [SerializeField] private DoorFlick closeLFlick = default;
    [SerializeField] private PointerEnterUI forward = default;
    [SerializeField] private float maxAlpha = 0.8f;

    public IObservable<Unit> ObserveGo => Observable.Merge(closeRFlick.UpSubject, closeLFlick.UpSubject);
    public IObservable<Unit> ObserveHandle => Observable.Merge(openFlick.RightSubject, openFlick.LeftSubject, closeRFlick.LeftSubject, closeLFlick.RightSubject);
    public IObservable<bool> ObserveHandOn => Observable.Merge(openFlick.IsHandOn, closeRFlick.IsHandOn, closeLFlick.IsHandOn);

    private RectTransform rectTransform;
    private RawImage rawImage;

    private float alpha = 0.0f;
    private bool isActive = false;
    private bool isOpen = false;
    private bool isFingerDown = false;

    private Vector2 UICenter;

    private Vector2 openCenter;
    private Vector2 closeRCenter;
    private Vector2 closeLCenter;
    private float openRadius;
    private float closeRRadius;
    private float closeLRadius;

    private bool InOpen(Vector2 screenPos) => (screenPos - openCenter).magnitude < openRadius;
    private bool InCloseR(Vector2 screenPos) => (screenPos - closeRCenter).magnitude < closeRRadius;
    private bool InCloseL(Vector2 screenPos) => (screenPos - closeLCenter).magnitude < closeLRadius;

    private FlickInteraction currentFlick = null;
    private Vector2 pressPos = Vector2.zero;

    private bool IsPressed => currentFlick != null;

    private Vector2 DragVector(Vector2 screenPos) => screenPos - pressPos;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rawImage = GetComponent<RawImage>();
    }

    void Start()
    {
        ResetCenterPos();

        openRadius = openDoorUI.Radius;
        closeRRadius = closeRDoorUI.Radius;
        closeLRadius = closeLDoorUI.Radius;

        Debug.Log("openRadius: " + openRadius);
        Debug.Log("closeRRadius: " + closeRRadius);
        Debug.Log("closeLRadius: " + closeLRadius);

        Debug.Log("openCenter: " + openCenter);
        Debug.Log("closeRCenter: " + closeRCenter);
        Debug.Log("closeLCenter: " + closeLCenter);

        SetAlpha(0.0f);
        gameObject.SetActive(false);
        InactivateButtons();
    }

    void Update()
    {
        UpdateTransparent();
    }

    void LateUpdate()
    {
        Debug.Log("LateUpdate: " + IsPressed);
        if (IsPressed)
        {
            Debug.Log("Inactivate");
            forward.Inactivate();
        }
    }

    private void UpdateTransparent()
    {
        alpha += isActive ? 0.05f : -0.1f;

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

        SetAlpha(alpha);
    }

    private void SetAlpha(float alpha)
    {
        if (isOpen)
        {
            closeRDoorUI.SetAlpha(alpha);
            closeLDoorUI.SetAlpha(alpha);
        }
        else
        {
            openDoorUI.SetAlpha(alpha);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isFingerDown) return;

        isFingerDown = false;

        if (currentFlick == null)
        {
            RaycastEvent<IPointerUpHandler>(eventData, (handler, data) => handler.OnPointerUp(data as PointerEventData));
            return;
        }

        if (!isActive) return;

        if (currentFlick == closeLFlick) closeRDoorUI.Activate(alpha);
        if (currentFlick == closeRFlick) closeLDoorUI.Activate(alpha);

        currentFlick.Release(DragVector(eventData.position));
        currentFlick = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isFingerDown = true;

        FlickInteraction flick = GetFlick(eventData.position);

        if (flick == null)
        {
            RaycastEvent<IPointerDownHandler>(eventData, (handler, data) => handler.OnPointerDown(data as PointerEventData));
            return;
        }

        Debug.Log("OnPointerDown: " + gameObject.name);

        if (!isActive) return;

        pressPos = eventData.position;
        currentFlick = flick;

        if (currentFlick == closeLFlick) closeRDoorUI.Inactivate();
        if (currentFlick == closeRFlick) closeLDoorUI.Inactivate();

        currentFlick.UpdateImage(DragVector(eventData.position));
    }

    public void OnDrag(PointerEventData eventData)
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
        UICenter = rectTransform.anchoredPosition;

        openCenter = openDoorUI.Position;
        closeRCenter = closeRDoorUI.Position;
        closeLCenter = closeLDoorUI.Position;
    }

    private FlickInteraction GetFlick(Vector2 screenPos)
    {
        Debug.Log("GetFlick: " + screenPos);

        if (isOpen)
        {
            if (InCloseR(screenPos)) return closeRFlick;
            if (InCloseL(screenPos)) return closeLFlick;
        }
        else
        {
            if (InOpen(screenPos)) return openFlick;
        }

        return null;
    }

    public void Activate(bool isOpen)
    {
        if (isActive && this.isOpen == isOpen) return;

        this.isOpen = isOpen;
        isActive = true;
        gameObject.SetActive(true);

        ActivateButtons();

        // Debug.Log("Door Handler Active");
    }

    private void ActivateButtons()
    {
        alpha = 0.0f;

        if (isOpen)
        {
            openDoorUI.Inactivate();
            closeRDoorUI.Activate(alpha);
            closeLDoorUI.Activate(alpha);
        }
        else
        {
            openDoorUI.Activate(alpha);
            closeRDoorUI.Inactivate();
            closeLDoorUI.Inactivate();
        }
    }

    private void InactivateButtons()
    {
        openDoorUI.Inactivate();
        closeRDoorUI.Inactivate();
        closeLDoorUI.Inactivate();

        currentFlick?.Inactivate();
    }

    public void Inactivate()
    {
        if (!isActive) return;

        ButtonCancel(true);
        isActive = false;
    }

    private void ButtonCancel(bool isFadeOnly = false)
    {
        if (isFadeOnly)
        {
            currentFlick?.Inactivate();
        }
        else
        {
            currentFlick?.Cancel();
        }

        isFingerDown = false;
        currentFlick = null;
        pressPos = Vector2.zero;
    }

    public void SetActive(bool value, bool isOpen)
    {
        if (value)
        {
            Activate(isOpen);
        }
        else
        {
            Inactivate();
        }
    }

    private void RaycastEvent<T>(PointerEventData eventData, ExecuteEvents.EventFunction<T> eventFunc) where T : IEventSystemHandler
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