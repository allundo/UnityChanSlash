using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class FightCircle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private AttackButton jabButton = default;
    [SerializeField] private AttackButton straightButton = default;
    [SerializeField] private AttackButton kickButton = default;
    [SerializeField] private float maxAlpha = 0.8f;
    [SerializeField] private float attackCancelThreshold = 2.0f;

    public AttackButton JabButton => jabButton;
    public AttackButton StraightButton => straightButton;
    public AttackButton KickButton => kickButton;

    private RectTransform rectTransform;
    private RawImage rawImage;

    private float alpha = 0.0f;
    private bool isActive = false;
    private bool isFingerDown = false;

    private Vector2 UICenter;
    private Vector2 screenCenter;
    private Vector2 kickCenter;

    private bool InKick(Vector2 uiPos) => (uiPos - kickCenter).magnitude < 20.0f;

    private AttackButton currentButton = null;
    private Vector2 pressPos = Vector2.zero;

    private bool IsPressed => currentButton != null;

    private float DrawComponent(Vector2 delta) => IsPressed ? Vector2.Dot(pressPos.normalized, delta) : 0.0f;
    private float radius;

    private Vector2 UIPos(Vector2 screenPos) => screenPos - screenCenter;
    private bool InCircle(Vector2 screenPos) => UIPos(screenPos).magnitude < radius;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rawImage = GetComponent<RawImage>();

        radius = rectTransform.rect.height / 2 - 10;
        kickCenter = new Vector2(0, -(radius - 20.0f));

        UICenter = rectTransform.anchoredPosition;
        screenCenter = new Vector2(Screen.width / 2, Screen.height / 2) + UICenter;

        SetAlpha(0.0f);
        gameObject.SetActive(false);
    }

    void Update()
    {
        UpdateTransparent();
    }

    private void UpdateTransparent()
    {
        alpha += isActive ? 0.1f : -0.1f;

        if (alpha > maxAlpha)
        {
            alpha = maxAlpha;
            return;
        }

        if (alpha < 0.0f)
        {
            alpha = 0.0f;
            gameObject.SetActive(false);
            return;
        }

        SetAlpha(alpha);
    }

    private void SetAlpha(float alpha)
    {
        Color c = rawImage.color;
        rawImage.color = new Color(c.r, c.g, c.b, alpha);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isFingerDown) return;

        isFingerDown = false;

        if (!IsPressed && !InCircle(eventData.position))
        {
            RaycastEvent<IPointerUpHandler>(eventData, (handler, data) => handler.OnPointerUp(data as PointerEventData));
            return;
        }

        if (!isActive) return;

        currentButton.Release();
        currentButton = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isFingerDown = true;

        if (!InCircle(eventData.position))
        {
            RaycastEvent<IPointerDownHandler>(eventData, (handler, data) => handler.OnPointerDown(data as PointerEventData));
            return;
        }

        if (!isActive) return;

        pressPos = UIPos(eventData.position);
        currentButton = GetAttack(pressPos);

        currentButton.Activate(pressPos);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isFingerDown) return;

        if (DrawComponent(eventData.delta) < -attackCancelThreshold)
        {
            ButtonCancel();
            return;
        }

        if (!IsPressed)
        {
            RaycastEvent<IDragHandler>(eventData, (handler, data) => handler.OnDrag(data as PointerEventData));
            return;
        }
    }

    private AttackButton GetAttack(Vector2 uiPos)
    {
        if (InKick(uiPos)) return kickButton;
        return uiPos.x <= 0.0f ? jabButton : straightButton;
    }

    public void Activate()
    {
        if (isActive) return;

        isActive = true;
        gameObject.SetActive(true);

        Debug.Log("Fight Circle Active");
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
            currentButton?.Inactivate();
        }
        else
        {
            currentButton?.Cancel();
        }

        isFingerDown = false;
        currentButton = null;
        pressPos = Vector2.zero;
    }

    public void SetActive(bool value)
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
