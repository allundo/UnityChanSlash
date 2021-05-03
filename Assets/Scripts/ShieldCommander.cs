using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(ShieldAnimator))]
public abstract class ShieldCommander : MobCommander
{
    public ShieldAnimator shieldAnim { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        shieldAnim = GetComponent<ShieldAnimator>();
    }

    public virtual void SetEnemyDetected(bool isDetected)
    {
        IsAutoGuard = isDetected;
        shieldAnim.guard.Bool = IsManualGuard || IsAutoGuard;
    }
    public virtual void SetManualGuard(bool isGuard)
    {
        IsManualGuard = isGuard;
        shieldAnim.guard.Bool = IsManualGuard || IsAutoGuard;
    }

    protected abstract class ShieldCommand : Command
    {
        protected ShieldCommander shieldCommander;

        public ShieldCommand(ShieldCommander commander, float duration) : base(commander, duration)
        {
            shieldCommander = commander;
        }
    }


    protected class GuardCommand : ShieldCommand
    {
        public GuardCommand(ShieldCommander commander, float duration) : base(commander, duration) { }

        public override void Execute()
        {
            shieldCommander.SetManualGuard(true);

            SetValidateTimer();
            DOVirtual.DelayedCall(duration, () =>
            {
                shieldCommander.SetManualGuard(false);
                shieldCommander.DispatchCommand();
            });
        }
    }
}