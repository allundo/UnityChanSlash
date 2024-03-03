using UniRx;
using System;
using DG.Tweening;

public abstract class UndeadCommand : EnemyCommand
{
    protected IUndeadReactor undeadReact;
    protected IUndeadAnimator undeadAnim;

    public UndeadCommand(CommandTarget target, float duration, float validateTiming = 0.95f) : base(target, duration, validateTiming)
    {
        undeadReact = target.react as IUndeadReactor;
        undeadAnim = target.anim as IUndeadAnimator;
    }
}

public class Resurrection : UndeadCommand
{
    protected ICommand startMoving;
    public Resurrection(CommandTarget target, float duration = 64, ICommand startMoving = null) : base(target, duration)
    {
        this.startMoving = startMoving ?? new StartMoving(target);
    }

    public override IObservable<Unit> Execute()
    {
        ITile tile = map.OnTile;
        if (!tile.IsCharacterOn && tile.IsEnterable())
        {
            undeadAnim.resurrection.Fire();
            undeadAnim.die.Bool = undeadAnim.sleep.Bool = false;
            undeadReact.OnResurrection();
            map.SetObjectOn();
            enemyMap.SetOnEnemy();

            if (map.DestVec.magnitude > 0f)
            {
                target.interrupt.OnNext(Data(startMoving));
            }
            else
            {
                // Validate input if no Command is reserved.
                validateTween = ValidateTween().Play();
            }
        }
        else
        {
            target.interrupt.OnNext(Data(this));
        }

        return ObservableComplete();
    }

    public override ICommand GetContinuation()
    {
        CancelValidate();
        return new ResurrectionContinue(target, startMoving, RemainingDuration);
    }
}

public class ResurrectionContinue : Command
{
    private ICommand startMoving;
    public ResurrectionContinue(CommandTarget target, ICommand startMoving, float duration) : base(target, null, null, duration)
    {
        this.target = target;
        this.map = target.map;
        this.startMoving = startMoving;
    }

    protected override bool Action()
    {
        if (map.DestVec.magnitude > 0f)
        {
            target.interrupt.OnNext(Data(startMoving));
        }
        return true;
    }

    public override ICommand GetContinuation()
    {
        CancelValidate();
        return new ResurrectionContinue(target, startMoving, RemainingDuration);
    }
}

public class StartMoving : UndeadCommand
{
    public StartMoving(CommandTarget target, float duration = 72f) : base(target, duration) { }

    protected override float Speed => TILE_UNIT / duration;

    public override IObservable<Unit> Execute()
    {
        var dest = map.DestVec;
        var timeScale = dest.magnitude / TILE_UNIT;

        var move = tweenMove.Move(map.CurrentVec3Pos + dest, timeScale);
        var border = timeScale - 0.49f;

        playingTween = border > 0f ?
            DOTween.Sequence().Join(move).AppendInterval(border).AppendCallback(() => enemyMap.SetOnEnemy())
            : move;

        playingTween.Play();

        anim.speed.Float = Speed;
        completeTween = tweenMove.DelayedCall(timeScale, () => anim.speed.Float = 0f).Play();

        validateTween = DOVirtual.DelayedCall(timeScale * invalidDuration, () => target.validate.OnNext(false)).Play();

        return ObservableComplete(timeScale);
    }
}

public class UndeadSleep : UndeadCommand
{
    protected ICommand resurrection;
    public UndeadSleep(CommandTarget target, float duration = 300f, ICommand resurrection = null) : base(target, duration)
    {
        this.resurrection = resurrection ?? new Resurrection(target);
    }

    public override IObservable<Unit> Execute()
    {
        playingTween = tweenMove.TurnToDir(1f).Play();

        undeadAnim.die.Bool = undeadAnim.sleep.Bool = true;
        undeadReact.OnSleep();
        target.interrupt.OnNext(Data(resurrection));

        return ObservableComplete(); // Don't validate input
    }
}

public class UndeadQuickSleep : UndeadCommand
{
    protected ICommand resurrection;
    public UndeadQuickSleep(CommandTarget target, float duration = 60f, ICommand resurrection = null) : base(target, duration)
    {
        this.resurrection = resurrection ?? new Resurrection(target);
    }

    public override IObservable<Unit> Execute()
    {
        undeadAnim.die.Bool = undeadAnim.sleep.Bool = true;
        target.anim.SetSpeed(100f);
        target.interrupt.OnNext(Data(resurrection));

        return ExecOnCompleted(target.anim.Resume); // Don't validate input
    }
}
