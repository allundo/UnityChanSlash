using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(ShieldAnimator))]
public abstract class ShieldCommander : MobCommander
{
    public ShieldAnimator shieldAnim { get; protected set; }
    protected bool IsAutoGuard = false;
    protected bool IsManualGuard = false;

    protected bool IsGuardOn => IsManualGuard || IsAutoGuard;

    protected int shieldCount = 0;
    protected readonly int SHIELD_READY = 10;
    public bool IsShieldReady => shieldCount == SHIELD_READY;

    public virtual bool IsShieldEnable => false; // IsIdling || currentInput == guard;

    public bool IsShieldOn(Direction attackDir) => IsShieldEnable && IsShieldReady && map.dir.IsInverse(attackDir);

    protected override void Awake()
    {
        base.Awake();
        shieldAnim = anim as ShieldAnimator;
    }
    protected override void Update()
    {
        base.Update();
        ShieldCountUp();
    }

    private void ShieldCountUp()
    {
        if (IsGuardOn)
        {
            if (shieldCount < SHIELD_READY) shieldCount++;
            return;
        }

        shieldCount = 0;
    }

    public virtual void SetEnemyDetected(bool isDetected)
    {
        IsAutoGuard = isDetected;
        shieldAnim.guard.Bool = IsGuardOn;
    }
    public virtual void SetManualGuard(bool isGuard)
    {
        IsManualGuard = isGuard;
        shieldAnim.guard.Bool = IsGuardOn;
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
            SetDispatchFinal(() => shieldCommander.SetManualGuard(false));
        }
    }
}
