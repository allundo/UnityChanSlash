using UnityEngine;
using System;
using UniRx;

public class AttackButtonsHandler : MonoBehaviour
{
    [SerializeField] private AttackButton jabButton = default;
    [SerializeField] private AttackButton straightButton = default;
    [SerializeField] private AttackButton kickButton = default;
    [SerializeField] private float kickRadius = 100f;

    protected Target enemyTarget;
    public void SetTarget(Target enemyTarget) => this.enemyTarget = enemyTarget;

    private ICommand jabCmd;
    private ICommand straightCmd;
    private ICommand kickCmd;

    private ICommand jabCriticalCmd;
    private ICommand straightCriticalCmd;
    private ICommand kickCriticalCmd;

    public void SetCommands(PlayerCommandTarget target)
    {
        jabCmd = new PlayerJab(target, 21.6f);
        straightCmd = new PlayerStraight(target, 30f);
        kickCmd = new PlayerKick(target, 43f);
        jabCriticalCmd = new PlayerJabCritical(target, 18.5f);
        straightCriticalCmd = new PlayerStraightCritical(target, 24f);
        kickCriticalCmd = new PlayerKickCritical(target, 35f);
    }

    public void SetRegions(InputRegion region)
    {
        jabButton.SetRegion(region.JabRegion);
        straightButton.SetRegion(region.StraightRegion);
        kickButton.SetRegion(region.KickRegion);
    }

    public IObservable<Unit> JabButton => jabButton.ObservableAtk;
    public IObservable<Unit> StraightButton => straightButton.ObservableAtk;
    public IObservable<Unit> KickButton => kickButton.ObservableAtk;

    public IObservable<ICommand> AttackButtons
        => Observable.Merge(
            JabButton.Select(_ => enemyTarget.isPointerOn ? jabCriticalCmd : jabCmd),
            StraightButton.Select(_ => enemyTarget.isPointerOn ? straightCriticalCmd : straightCmd),
            KickButton.Select(_ => enemyTarget.isPointerOn ? kickCriticalCmd : kickCmd)
        );

    private Vector2 kickUICenter;
    public void SetUIRadius(float radius)
    {
        kickUICenter = new Vector2(0, -(radius - kickRadius));
    }

    private float sqrKickRadius;
    private bool InKick(Vector2 uiPos) => (kickUICenter - uiPos).sqrMagnitude < sqrKickRadius;

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

    public AttackButton GetAttack(Vector2 uiPos)
    {
        if (InKick(uiPos)) return kickButton;
        return uiPos.x <= 0.0f ? jabButton : straightButton;
    }
}