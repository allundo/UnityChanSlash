﻿using UnityEngine;
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
    [SerializeField] private Target enemyTarget = default;
    [SerializeField] private PivotPoint pivotPoint = default;
    [SerializeField] private EffortPoint effortPoint = default;
    [SerializeField] private TargetPointer targetPointer = default;

    private ICommand jabCommand;
    private ICommand straightCommand;
    private ICommand kickCommand;

    public IObservable<Unit> JabButton => jabButton.ObservableAtk;
    public IObservable<Unit> StraightButton => straightButton.ObservableAtk;
    public IObservable<Unit> KickButton => kickButton.ObservableAtk;

    public IObservable<ICommand> AttackButtons
        => Observable.Merge(
            JabButton.Select(_ => jabCommand),
            StraightButton.Select(_ => straightCommand),
            KickButton.Select(_ => kickCommand)
        );

    private RectTransform rectTransform;
    private RaycastHandler raycastHandler;

    private float alpha = 0.0f;
    public bool isActive { get; private set; } = false;
    private bool isFingerDown = false;
    public bool isForwardMovable = false;

    private Vector2 UICenter;
    private Vector2 kickUICenter;
    private Vector2 forwardUICenter;

    private bool InKick(Vector2 uiPos) => (kickUICenter - uiPos).magnitude < 100.0f;
    private bool InForward(Vector2 screenPos) => (forwardUICenter - UIPos(screenPos)).magnitude < 80.0f;

    private AttackButton currentButton = null;
    private Vector2 pressPos = Vector2.zero;

    public bool IsPressed => currentButton != null;
    public bool isChargingUp { get; private set; }

    private float DrawComponent(Vector2 screenPos) => IsPressed ? Vector2.Dot(UIPos(pressPos).normalized, DragVector(screenPos)) : 0.0f;
    private float radius;
    private float sqrRadius;

    private Vector2 UIPos(Vector2 screenPos) => screenPos - UICenter;
    private Vector2 DragVector(Vector2 screenPos) => screenPos - pressPos;
    private bool InCircle(Vector2 screenPos) => UIPos(screenPos).sqrMagnitude < sqrRadius;

    private IReactiveProperty<IEnemyStatus> EnemyStatus = new ReactiveProperty<IEnemyStatus>(null);

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        raycastHandler = new RaycastHandler(gameObject);
    }

    public void SetCommands(PlayerCommandTarget target)
    {
        jabCommand = new PlayerJab(target, 21.6f);
        straightCommand = new PlayerStraight(target, 30f);
        kickCommand = new PlayerKick(target, 43f);
    }

    void Start()
    {
        radius = 260.0f;
        sqrRadius = radius * radius;
        kickUICenter = new Vector2(0, -(radius - 100.0f));
        forwardUICenter = new Vector2(0, 12f);

        ResetCenterPos();

        circle.SetAlpha(0.0f);
        gameObject.SetActive(false);

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

        JabButton.Subscribe(_ =>
        {
            straightButton.SetCoolTime(jabButton.CancelTime);
            kickButton.SetCoolTime(jabButton.CancelTime);
        })
        .AddTo(this);

        StraightButton.Subscribe(_ =>
        {
            jabButton.SetCoolTime(straightButton.CancelTime);
            kickButton.SetCoolTime(straightButton.CoolTime);
        })
        .AddTo(this);

        KickButton.Subscribe(_ =>
        {
            jabButton.SetCoolTime(kickButton.CoolTime);
            straightButton.SetCoolTime(kickButton.CoolTime);
        })
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

        if (!IsPressed)
        {
            if (!InCircle(eventData.position))
            {
                raycastHandler.RaycastPointerUp(eventData);
                return;
            }

            if (isForwardMovable && InForward(eventData.position))
            {
                raycastHandler.RaycastPointerExit(eventData);
                return;
            }
        }

        if (!isActive) return;

        currentButton?.Release();
        currentButton = null;

        pivotPoint.Hide();
        effortPoint.Hide();
        targetPointer.Hide();
        enemyTarget.SetPointer(Vector2.zero);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isFingerDown) return;

        isFingerDown = true;

        if (!InCircle(eventData.position))
        {
            raycastHandler.RaycastPointerDown(eventData);
            return;
        }

        if (isForwardMovable && InForward(eventData.position))
        {
            raycastHandler.RaycastPointerEnter(eventData);
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
            raycastHandler.RaycastDrag(eventData);
            return;
        }

        pivotPoint.Show(pressPos);
        effortPoint.Show(eventData.position);
        targetPointer.Show(pressPos);

        var pointerVec = pressPos - eventData.position;
        targetPointer.SetVerticesPos(pointerVec);

        if (InCircle(eventData.position))
        {
            pivotPoint.DisableChargingUp();
            effortPoint.DisableChargingUp();
            targetPointer.DisableChargingUp();
        }
        else
        {
            pivotPoint.EnableChargingUp();
            effortPoint.EnableChargingUp();
            targetPointer.EnableChargingUp();
            enemyTarget.SetPointer(pressPos + pointerVec);
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

    public void Activate(IEnemyStatus status)
    {
        EnemyStatus.Value = status;

        if (isActive) return;

        isActive = true;
        gameObject.SetActive(true);
        enemyTarget.FadeActivate(status);
    }

    public void Inactivate(bool isForce = false)
    {
        if (!isForce && IsPressed || !isActive) return;

        ButtonCancel(true);
        isActive = false;
        enemyTarget.FadeInactivate();
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

        pivotPoint.Hide();
        effortPoint.Hide();
        targetPointer.Hide();
        enemyTarget.SetPointer(Vector2.zero);

        var eventData = new PointerEventData(EventSystem.current);
        eventData.pressPosition = pressPos;
        raycastHandler.RaycastPointerUp(eventData);

        currentButton = null;
        pressPos = Vector2.zero;
    }

    public void SetActive(bool value, IEnemyStatus status)
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
