using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using UniRx;
using System;

[RequireComponent(typeof(MaskableGraphic))]
public class FightCircle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private AttackButton jabButton = default;
    [SerializeField] private AttackButton straightButton = default;
    [SerializeField] private AttackButton kickButton = default;
    [SerializeField] private float maxAlpha = 0.8f;
    [SerializeField] private float attackCancelThreshold = 2.0f;

    [SerializeField] private EnemyLifeGauge circle = default;

    public IObservable<Unit> JabButton => jabButton.ObservableAtk;
    public IObservable<Unit> StraightButton => straightButton.ObservableAtk;
    public IObservable<Unit> KickButton => kickButton.ObservableAtk;

    private RectTransform rectTransform;
    private RaycastHandler raycastHandler;

    private float alpha = 0.0f;
    public bool isActive { get; private set; } = false;
    private bool isFingerDown = false;

    private Vector2 UICenter;
    private Vector2 kickUICenter;

    private bool InKick(Vector2 uiPos) => (kickUICenter - uiPos).magnitude < 100.0f;

    private AttackButton currentButton = null;
    private Vector2 pressPos = Vector2.zero;

    private bool IsPressed => currentButton != null;

    private float DrawComponent(Vector2 screenPos) => IsPressed ? Vector2.Dot(UIPos(pressPos).normalized, DragVector(screenPos)) : 0.0f;
    private float radius;

    private Vector2 UIPos(Vector2 screenPos) => screenPos - UICenter;
    private Vector2 DragVector(Vector2 screenPos) => screenPos - pressPos;
    private bool InCircle(Vector2 screenPos) => UIPos(screenPos).magnitude < radius;

    private IReactiveProperty<MobStatus> EnemyStatus = new ReactiveProperty<MobStatus>(null);

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        raycastHandler = new RaycastHandler(gameObject);
    }

    void Start()
    {
        radius = 260.0f;
        kickUICenter = new Vector2(0, -(radius - 100.0f));

        ResetCenterPos();

        circle.SetAlpha(0.0f);
        gameObject.SetActive(false);

        // CurrentButton.Subscribe(button => UIMask.SetActive(button != null)).AddTo(this);

        EnemyStatus
            .Where(status => status != null)
            .SelectMany(status =>
            {
                circle.OnEnemyChange(status.Life.Value, status.LifeMax.Value);
                return status.Life;
            })
            .TakeUntil(EnemyStatus.Skip(1))
            .RepeatUntilDestroy(gameObject)
            .Subscribe(life => circle.OnLifeChange(life))
            .AddTo(this);
    }

    void Update()
    {
        UpdateTransparent();
    }

    private void UpdateTransparent()
    {
        alpha += (isActive ? 6f : -6f) * Time.deltaTime;

        if (alpha > maxAlpha)
        {
            alpha = maxAlpha;
        }

        if (alpha < 0.0f)
        {
            alpha = 0.0f;
            gameObject.SetActive(false);
        }

        circle.SetAlpha(alpha);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isFingerDown) return;

        isFingerDown = false;

        if (!IsPressed && !InCircle(eventData.position))
        {
            raycastHandler.RaycastEvent<IPointerUpHandler>(eventData, (handler, data) => handler.OnPointerUp(data as PointerEventData));
            return;
        }

        if (!isActive) return;

        currentButton?.Release();
        currentButton = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isFingerDown = true;

        if (!InCircle(eventData.position))
        {
            raycastHandler.RaycastEvent<IPointerDownHandler>(eventData, (handler, data) => handler.OnPointerDown(data as PointerEventData));
            return;
        }

        if (!isActive) return;

        pressPos = eventData.position;
        currentButton = GetAttack(UIPos(eventData.position));

        currentButton.Press(pressPos);
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
            raycastHandler.RaycastEvent<IDragHandler>(eventData, (handler, data) => handler.OnDrag(data as PointerEventData));
            return;
        }
    }

    public void ResetCenterPos()
    {
        UICenter = rectTransform.position;
    }

    private AttackButton GetAttack(Vector2 uiPos)
    {
        if (InKick(uiPos)) return kickButton;
        return uiPos.x <= 0.0f ? jabButton : straightButton;
    }

    public void Activate(MobStatus status)
    {
        EnemyStatus.Value = status;

        if (isActive) return;

        isActive = true;
        gameObject.SetActive(true);
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
            currentButton?.FadeOut()?.Play();
        }
        else
        {
            currentButton?.Cancel();
        }

        var eventData = new PointerEventData(EventSystem.current);
        eventData.pressPosition = pressPos;
        raycastHandler.RaycastEvent<IPointerUpHandler>(eventData, (handler, data) => handler.OnPointerUp(data as PointerEventData));

        currentButton = null;
        pressPos = Vector2.zero;
    }

    public void SetActive(bool value, MobStatus status)
    {
        if (value)
        {
            Activate(status);
        }
        else
        {
            Inactivate();
        }
    }
}
