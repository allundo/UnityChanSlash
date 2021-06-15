using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using UniRx;

public class DoorHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private HandleButton openButton = default;
    [SerializeField] private HandleButton closeRButton = default;
    [SerializeField] private HandleButton closeLButton = default;
    [SerializeField] private DoorFlick openFlick = default;
    [SerializeField] private DoorFlick closeRFlick = default;
    [SerializeField] private DoorFlick closeLFlick = default;
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
    private Vector2 screenCenter;

    private Vector2 openCenter;
    private Vector2 closeRCenter;
    private Vector2 closeLCenter;
    private float openRadius;
    private float closeRRadius;
    private float closeLRadius;

    private bool InOpen(Vector2 uiPos) => (uiPos - openCenter).magnitude < openRadius;
    private bool InCloseR(Vector2 uiPos) => (uiPos - closeRCenter).magnitude < closeRRadius;
    private bool InCloseL(Vector2 uiPos) => (uiPos - closeLCenter).magnitude < closeLRadius;

    private FlickInteraction currentFlick = null;
    private Vector2 pressPos = Vector2.zero;

    private bool IsPressed => currentFlick != null;

    private Vector2 UIPos(Vector2 screenPos) => screenPos - screenCenter - UICenter;
    private Vector2 ScreenVec(Vector2 screenPos) => screenPos - screenCenter;
    private Vector2 DragVector(Vector2 screenPos) => screenPos - pressPos;


    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rawImage = GetComponent<RawImage>();

        UICenter = rectTransform.anchoredPosition;
        screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        openCenter = openButton.Position;
        closeRCenter = closeRButton.Position;
        closeLCenter = closeLButton.Position;

        Debug.Log("openCenter: " + openCenter);
        Debug.Log("closeRCenter: " + closeRCenter);
        Debug.Log("closeLCenter: " + closeLCenter);

        openRadius = openButton.Radius;
        closeRRadius = closeRButton.Radius;
        closeLRadius = closeLButton.Radius;

        Debug.Log("openRadius: " + openRadius);
        Debug.Log("closeRRadius: " + closeRRadius);
        Debug.Log("closeLRadius: " + closeLRadius);

        SetAlpha(0.0f);
        gameObject.SetActive(false);
        InactivateButtons();
    }

    void Update()
    {
        UpdateTransparent();
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
            closeRButton.SetAlpha(alpha);
            closeLButton.SetAlpha(alpha);
        }
        else
        {
            openButton.SetAlpha(alpha);
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

        currentFlick.Release(DragVector(eventData.position));
        currentFlick = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isFingerDown = true;

        FlickInteraction flick = GetFlick(ScreenVec(eventData.position));

        if (flick == null)
        {
            RaycastEvent<IPointerDownHandler>(eventData, (handler, data) => handler.OnPointerDown(data as PointerEventData));
            return;
        }

        if (!isActive) return;

        pressPos = eventData.position;
        currentFlick = flick;

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

    private FlickInteraction GetFlick(Vector2 uiPos)
    {
        if (isOpen)
        {
            if (InCloseR(uiPos)) return closeRFlick;
            if (InCloseL(uiPos)) return closeLFlick;
        }
        else
        {
            if (InOpen(uiPos)) return openFlick;
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
            openButton.Inactivate();
            closeRButton.Activate(alpha);
            closeLButton.Activate(alpha);
        }
        else
        {
            openButton.Activate(alpha);
            closeRButton.Inactivate();
            closeLButton.Inactivate();
        }
    }

    private void InactivateButtons()
    {
        openButton.Inactivate();
        closeRButton.Inactivate();
        closeLButton.Inactivate();

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