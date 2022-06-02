using UniRx;
using System;
using DG.Tweening;

public abstract class UndeadCommand : EnemyCommand
{
    protected IUndeadReactor undeadReact;
    protected IUndeadAnimator undeadAnim;

    public UndeadCommand(ICommandTarget target, float duration, float validateTiming = 0.95f) : base(target, duration, validateTiming)
    {
        undeadReact = target.react as IUndeadReactor;
        undeadAnim = target.anim as IUndeadAnimator;
    }
}

public class Resurrection : UndeadCommand
{
    private ICommand startMoving;
    public Resurrection(ICommandTarget target, float duration) : base(target, duration)
    {
        startMoving = new StartMoving(target, 72f);
    }

    public override IObservable<Unit> Execute()
    {
        if (map.OnTile.IsEnterable())
        {
            undeadAnim.resurrection.Fire();
            undeadAnim.die.Bool = undeadAnim.sleep.Bool = false;
            undeadReact.OnResurrection();
            map.SetObjectOn();
            enemyMap.SetOnEnemy();

            if (map.DestVec.magnitude > 0f)
            {
                input.Interrupt(startMoving, false);
            }
            else
            {
                // Validate input if no Command is reserved.
                validateTween = ValidateTween().Play();
            }
        }
        else
        {
            input.Interrupt(this, false);
        }

        return ObservableComplete();
    }
}

public class StartMoving : UndeadCommand
{
    public StartMoving(ICommandTarget target, float duration) : base(target, duration) { }

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

        validateTween = DOVirtual.DelayedCall(timeScale * invalidDuration, () => input.ValidateInput()).Play();

        return ObservableComplete(timeScale);
    }
}

public class UndeadSleep : UndeadCommand
{
    ICommand resurrection;
    public UndeadSleep(ICommandTarget target, float duration, ICommand resurrection) : base(target, duration)
    {
        this.resurrection = resurrection;
    }

    public override IObservable<Unit> Execute()
    {
        undeadAnim.die.Bool = undeadAnim.sleep.Bool = true;
        undeadReact.OnSleep();
        input.Interrupt(resurrection, false);

        return ObservableComplete(); // Don't validate input
    }
}
