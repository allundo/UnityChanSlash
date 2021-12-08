using UniRx;
using DG.Tweening;

public abstract class ShieldInput : MobInput
{
    public GuardState guardState { get; protected set; }

    protected bool IsShield => commander.currentCommand is ShieldCommand;

    public override bool IsFightValid => IsIdling || IsShield;

    protected virtual void Start()
    {
        SetInputs();
    }

    protected override void SetCommander()
    {
        commander = new ShieldCommander(target);
    }

    protected override void SetCommands()
    {
        die = new DieCommand(target, 0.1f);
    }

    /// <summary>
    /// This method is called by Start(). Override it for UniRx subscription. <br />
    /// Mainly used for input definition.
    /// </summary>
    protected virtual void SetInputs()
    {
        guardState = new GuardState(this);
    }

    public override void InputCommand(Command cmd)
    {
        if (!isCommandValid || cmd == null) return;

        isCommandValid = false;

        commander.EnqueueCommand(cmd);
    }

    public class GuardState
    {
        private ShieldInput input;
        private ShieldAnimator anim;
        private MapUtil map;
        private Command shieldOn;
        private float timeToReady;
        public bool isShieldReady = false;

        private IReactiveProperty<bool> IsAutoGuard = new ReactiveProperty<bool>(false);

        private bool isAutoGuard
        {
            get { return IsAutoGuard.Value; }
            set { IsAutoGuard.Value = value; }
        }

        private Tween readyTween = null;
        private void SetShieldReady(bool isGuardOn)
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

        public bool IsShieldOn(IDirection attackDir) => input.IsFightValid && isShieldReady && map.dir.IsInverse(attackDir);

        public GuardState(ShieldInput input, float duration = 15f, float timeToReady = 0.16f)
        {
            this.input = input;
            this.timeToReady = timeToReady;


            var target = input.target;

            map = target.map;
            anim = target.anim as ShieldAnimator;

            shieldOn = new ShieldOnCommand(target, this, duration);

            var IsShieldObservable = (input.commander as ShieldCommander)
                .CurrentObservable
                .Select(cmd => cmd is ShieldCommand);

            Observable.Merge(IsAutoGuard, IsShieldObservable)
                .Select(_ => isAutoGuard || input.IsShield)
                .Subscribe(isGuardOn => SetShieldReady(isGuardOn))
                .AddTo(target);
        }

        public void SetShield() => input.ForceEnqueue(shieldOn, true);

        public void SetEnemyDetected(bool isDetected)
        {
            isAutoGuard = isDetected;
        }
    }
}
