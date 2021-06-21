using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UniRx;

public class FightCircle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private AttackButton jabButton = default;
    [SerializeField] private AttackButton straightButton = default;
    [SerializeField] private AttackButton kickButton = default;
    [SerializeField] private float maxAlpha = 0.8f;
    [SerializeField] private float attackCancelThreshold = 2.0f;
    [SerializeField] private GameObject UIMask = default;

    [SerializeField] private EnemyLifeGauge circle = default;

    public AttackButton JabButton => jabButton;
    public AttackButton StraightButton => straightButton;
    public AttackButton KickButton => kickButton;

    private RectTransform rectTransform;
    private Image image;

    private float alpha = 0.0f;
    public bool isActive { get; private set; } = false;
    private bool isFingerDown = false;

    private Vector2 UICenter;
    private Vector2 screenCenter;
    private Vector2 kickCenter;

    private bool InKick(Vector2 uiPos) => (uiPos - kickCenter).magnitude < 20.0f;

    private IReactiveProperty<AttackButton> CurrentButton = new ReactiveProperty<AttackButton>(null);
    private Vector2 pressPos = Vector2.zero;

    private bool IsPressed => CurrentButton.Value != null;

    private float DrawComponent(Vector2 screenPos) => IsPressed ? Vector2.Dot(UIPos(pressPos).normalized, DragVector(screenPos)) : 0.0f;
    private float radius;

    private Vector2 UIPos(Vector2 screenPos) => screenPos - screenCenter - UICenter;
    private Vector2 ScreenVec(Vector2 screenPos) => screenPos - screenCenter;
    private Vector2 DragVector(Vector2 screenPos) => screenPos - pressPos;
    private bool InCircle(Vector2 screenPos) => UIPos(screenPos).magnitude < radius;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        radius = 260.0f;
        kickCenter = new Vector2(0, -(radius - 20.0f));

        UICenter = rectTransform.anchoredPosition;
        screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        circle.SetAlpha(0.0f);
        gameObject.SetActive(false);

        CurrentButton.Subscribe(button => UIMask.SetActive(button != null));
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

        circle.SetAlpha(alpha);
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

        CurrentButton.Value?.Release();
        CurrentButton.Value = null;
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

        pressPos = eventData.position;
        CurrentButton.Value = GetAttack(UIPos(eventData.position));

        CurrentButton.Value.Activate(ScreenVec(pressPos));
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isFingerDown) return;

        if (DrawComponent(eventData.position) < -attackCancelThreshold)
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
            CurrentButton.Value?.Inactivate();
        }
        else
        {
            CurrentButton.Value?.Cancel();
        }

        isFingerDown = false;
        CurrentButton.Value = null;
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
        image.raycastTarget = false;

        EventSystem.current.RaycastAll(eventData, objectsHit);

        image.raycastTarget = true;

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
