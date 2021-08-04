﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
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
    private Vector2 kickUICenter;

    private bool InKick(Vector2 uiPos) => (kickUICenter - uiPos).magnitude < 20.0f;

    private IReactiveProperty<AttackButton> CurrentButton = new ReactiveProperty<AttackButton>(null);
    private Vector2 pressPos = Vector2.zero;

    private bool IsPressed => CurrentButton.Value != null;

    private float DrawComponent(Vector2 screenPos) => IsPressed ? Vector2.Dot(UIPos(pressPos).normalized, DragVector(screenPos)) : 0.0f;
    private float radius;

    private Vector2 UIPos(Vector2 screenPos) => screenPos - UICenter;
    private Vector2 DragVector(Vector2 screenPos) => screenPos - pressPos;
    private bool InCircle(Vector2 screenPos) => UIPos(screenPos).magnitude < radius;

    private IReactiveProperty<MobStatus> EnemyStatus = new ReactiveProperty<MobStatus>(null);

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    void Start()
    {
        radius = 260.0f;
        kickUICenter = new Vector2(0, -(radius - 20.0f));

        ResetCenterPos();

        circle.SetAlpha(0.0f);
        gameObject.SetActive(false);

        CurrentButton.Subscribe(button => UIMask.SetActive(button != null));

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
        alpha += isActive ? 0.1f : -0.1f;

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

        CurrentButton.Value.Activate(pressPos);
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