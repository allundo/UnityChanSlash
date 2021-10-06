using UniRx;
using DG.Tweening;

public class GuardStateTemp
{
    private ShieldInput input;
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

    public bool IsShieldOn(IDirection attackDir) => input.IsFightValid && isShieldReady && map.dir.IsInverse(attackDir);

    public GuardStateTemp(ShieldInput input, float duration = 0.42f, float timeToReady = 0.16f)
    {
        this.input = input;
        this.timeToReady = timeToReady;


        var target = input.target;

        map = target.map;
        anim = target.anim as ShieldAnimator;

        shieldOn = new ShieldOnCommand(target, this, duration);

        Observable.Merge(IsAutoGuard, IsManualGuard)
            .Select(_ => isManualGuard || isAutoGuard)
            .Subscribe(isGuardOn => SetShieldReady(isGuardOn))
            .AddTo(target);
    }

    public void SetShield() => input.ForceEnqueue(shieldOn, true);

    public void SetEnemyDetected(bool isDetected)
    {
        isAutoGuard = isDetected;
    }

    public void SetManualGuard(bool isGuard)
    {
        isManualGuard = isGuard;
    }
}