using DG.Tweening;
using UniRx;
using System;

public abstract class ShieldInput : MobInput
{
    public GuardState guardState { get; protected set; }

    public bool IsShield => commander.currentCommand is ShieldCommand;

    public bool IsFightValid => IsIdling || IsShield;

    protected override void SetCommander()
    {
        commander = new ShieldCommander(gameObject);
    }

    protected override void Start()
    {
        base.Start();
        SetInputs();
    }

    /// <summary>
    /// This method is called by Start(). Override it for UniRx subscription. <br />
    /// Mainly used for input definition.
    /// </summary>
    protected virtual void SetInputs()
    {
        guardState = new GuardState(this);
    }

    public override ICommand InputIced(float duration)
    {
        if (commander.currentCommand is ShieldOnCommand) ClearAll();
        return base.InputIced(duration);
    }

    public class GuardState
    {
        protected ShieldInput input;
        protected IMapUtil map;
        protected ShieldAnimator anim;
        protected ICommand shieldOn;
        protected float timeToReady;
        public bool isShieldReady = false;

        protected IObservable<bool> IsShieldObservable
            => (input.commander as ShieldCommander)
                .CurrentObservable
                .Select(cmd => cmd is ShieldCommand);

        public GuardState(ShieldInput input, float duration = 15f, float timeToReady = 0.1f)
        {
            this.input = input;
            this.timeToReady = timeToReady;
            map = input.target.map;
            shieldOn = new ShieldOnCommand(input.target, duration);
            anim = input.target.anim as ShieldAnimator;

            Subscribe(input);
        }

        protected virtual void Subscribe(ShieldInput input)
        {
            IsShieldObservable
                .Subscribe(isGuardOn => SetShieldReady(isGuardOn))
                .AddTo(input.gameObject);
        }

        public virtual bool IsShieldOn(IDirection attackDir) => isShieldReady && map.dir.IsInverse(attackDir);

        /// <summary>
        /// Input ShieldCommand and execute immediately
        /// </summary>
        /// <returns>Effectiveness of shield</returns>
        public virtual float SetShield()
        {
            input.ClearAll(true, false, 9);
            input.Interrupt(shieldOn);
            return 1f;
        }

        protected Tween readyTween = null;
        protected void SetShieldReady(bool isGuardOn)
        {
            anim.guard.Bool = isGuardOn;

            if (isGuardOn)
            {
                readyTween = DOVirtual.DelayedCall(timeToReady, () => isShieldReady = true, false).Play();
            }
            else
            {
                readyTween?.Kill();
                isShieldReady = false;
            }
        }

    }
}