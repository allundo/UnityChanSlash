using UniRx;
using DG.Tweening;

public class GuardState
{
    private ShieldCommander commander;
    private ShieldAnimator anim;
    private MapUtil map;
    private Command shieldOn;
    private float timeToReady;
    public bool isShieldReady = false;

    private IReactiveProperty<bool> IsAutoGuard = new ReactiveProperty<bool>(false);
    private IReactiveProperty<bool> IsManualGuard = new ReactiveProperty<bool>(false);

    private bool isAutoGuard
    {
        get { return IsAutoGuard.Value; }
        set { IsAutoGuard.Value = value; }
    }
    private bool isManualGuard
    {
        get { return IsManualGuard.Value; }
        set { IsManualGuard.Value = value; }
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

    public bool IsShieldOn(IDirection attackDir) => commander.IsFightValid && isShieldReady && map.dir.IsInverse(attackDir);

    public GuardState(ShieldCommander commander, float duration = 0.42f, float timeToReady = 0.16f)
    {
        this.commander = commander;
        this.timeToReady = timeToReady;

        map = commander.map;
        anim = commander.anim as ShieldAnimator;

        shieldOn = new ShieldOnCommand(commander, duration, this);

        Observable.Merge(IsAutoGuard, IsManualGuard)
            .Select(_ => isManualGuard || isAutoGuard)
            .Subscribe(isGuardOn => SetShieldReady(isGuardOn))
            .AddTo(commander);
    }

    public void SetShield() => commander.EnqueueCommand(shieldOn, true);

    public void SetEnemyDetected(bool isDetected)
    {
        isAutoGuard = isDetected;
    }

    public void SetManualGuard(bool isGuard)
    {
        isManualGuard = isGuard;
    }
}