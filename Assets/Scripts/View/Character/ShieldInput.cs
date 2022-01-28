using DG.Tweening;
using UniRx;
using System;

public abstract class ShieldInput : MobInput
{
    public GuardState guardState { get; protected set; }

    public bool IsShield => commander.currentCommand is ShieldCommand;

    public override bool IsFightValid => IsIdling || IsShield;

    protected virtual void Start()
    {
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
    protected override void SetCommander()
    {
        commander = new ShieldCommander(target);
    }

    public class GuardState
    {
        protected ShieldInput input;
        protected IMapUtil map;
        protected ShieldAnimator anim;
        private ICommand shieldOn;
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
                .AddTo(input.target);
        }

        public virtual bool IsShieldOn(IDirection attackDir) => isShieldReady && map.dir.IsInverse(attackDir);
        public void SetShield()
        {
            input.ClearAll(true);
            input.Interrupt(shieldOn);
        }

        protected Tween readyTween = null;
        protected virtual void SetShieldReady(bool isGuardOn)
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