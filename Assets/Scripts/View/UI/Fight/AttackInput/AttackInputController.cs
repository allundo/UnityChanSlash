using UnityEngine;
using DG.Tweening;
using UniRx;
using System;

public class AttackInputController : MonoBehaviour
{
    [SerializeField] private AttackButton jabButton = default;
    [SerializeField] private AttackButton straightButton = default;
    [SerializeField] private AttackButton kickButton = default;

    [SerializeField] private Target enemyTarget = default;
    [SerializeField] private PivotPoint pivotPoint = default;
    [SerializeField] private EffortPoint effortPoint = default;
    [SerializeField] private TargetPointer targetPointer = default;

    [SerializeField] private float attackCancelThreshold = 2.0f;
    [SerializeField] private float kickRadius = 100f;

    private ICommand jabCmd;
    private ICommand straightCmd;
    private ICommand kickCmd;

    private ICommand jabCriticalCmd;
    private ICommand straightCriticalCmd;
    private ICommand kickCriticalCmd;

    public IObservable<Unit> JabButton => jabButton.ObservableAtk;
    public IObservable<Unit> StraightButton => straightButton.ObservableAtk;
    public IObservable<Unit> KickButton => kickButton.ObservableAtk;

    public IObservable<ICommand> AttackButtons
        => Observable.Merge(
            JabButton.Select(_ => enemyTarget.isPointerOn ? jabCriticalCmd : jabCmd),
            StraightButton.Select(_ => enemyTarget.isPointerOn ? straightCriticalCmd : straightCmd),
            KickButton.Select(_ => enemyTarget.isPointerOn ? kickCriticalCmd : kickCmd)
        );

    private Vector2 UICenter;
    private Vector2 kickUICenter;

    public void SetUICenter(Vector2 pos) => UICenter = pos;
    public void SetUIRadius(float radius)
    {
        sqrRadius = radius * radius;
        kickUICenter = new Vector2(0, -(radius - kickRadius));
    }

    private AttackButton currentButton = null;

    public Vector2 pressPos { get; private set; } = Vector2.zero;
    public bool IsPressed => currentButton != null;

    private float sqrRadius;

    private Vector2 UIPos(Vector2 screenPos) => screenPos - UICenter;
    private Vector2 DragVector(Vector2 screenPos) => screenPos - pressPos;
    private float DrawComponent(Vector2 screenPos) => IsPressed ? Vector2.Dot(UIPos(pressPos).normalized, DragVector(screenPos)) : 0.0f;
    public bool IsDragCancel(Vector2 screenPos) => DrawComponent(screenPos) < -attackCancelThreshold;

    public bool InCircle(Vector2 screenPos) => UIPos(screenPos).sqrMagnitude < sqrRadius;

    private float sqrKickRadius;
    private bool InKick(Vector2 uiPos) => (kickUICenter - uiPos).sqrMagnitude < sqrKickRadius;

    public void SetCommands(PlayerCommandTarget target)
    {
        jabCmd = new PlayerJab(target, 21.6f);
        straightCmd = new PlayerStraight(target, 30f);
        kickCmd = new PlayerKick(target, 43f);
        jabCriticalCmd = new PlayerJabCritical(target, 18.5f);
        straightCriticalCmd = new PlayerStraightCritical(target, 24f);
        kickCriticalCmd = new PlayerKickCritical(target, 35f);
    }

    void Start()
    {
        sqrKickRadius = kickRadius * kickRadius;

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

    public void Release()
    {
        currentButton?.Release();
        currentButton = null;

        pivotPoint.Hide();
        effortPoint.Hide();
        targetPointer.Hide();
        enemyTarget.SetPointer(Vector2.zero);
    }

    public void Press(Vector2 screenPos)
    {
        pressPos = screenPos;
        currentButton = GetAttack(UIPos(screenPos));

        currentButton.Press(pressPos);
    }

    public void ChargeUp(Vector2 screenPos)
    {
        pivotPoint.Show(pressPos);
        effortPoint.Show(screenPos);
        targetPointer.Show(pressPos);

        var pointerVec = pressPos - screenPos;
        targetPointer.SetVerticesPos(pointerVec);

        if (InCircle(screenPos))
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

    private AttackButton GetAttack(Vector2 uiPos)
    {
        if (InKick(uiPos)) return kickButton;
        return uiPos.x <= 0.0f ? jabButton : straightButton;
    }

    public void ButtonCancel(bool isFadeOnly = false)
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

        currentButton = null;
        pressPos = Vector2.zero;
    }
}
