using UniRx;
using System;
using DG.Tweening;

public abstract class UndeadCommand : EnemyCommand
{
    protected IUndeadReactor undeadReact;
    protected IUndeadAnimator undeadAnim;

    public UndeadCommand(EnemyCommandTarget target, float duration, float validateTiming = 0.95f) : base(target, duration, validateTiming)
    {
        undeadReact = target.react as IUndeadReactor;
        undeadAnim = target.anim as IUndeadAnimator;
    }
}

public class Resurrection : UndeadCommand
{
    private ICommand startMoving;
    public Resurrection(EnemyCommandTarget target, float duration) : base(target, duration)
    {
        startMoving = new StartMoving(target, 72f);
    }

    public override IObservable<Unit> Execute()
    {
        if (map.OnTile.IsCharacterOn)
        {
            input.Interrupt(this, false);
        }
        else
        {
            undeadAnim.resurrection.Fire();
            undeadAnim.sleep.Bool = false;
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

        return ObservableComplete();
    }
}

public class StartMoving : UndeadCommand
{
    public StartMoving(EnemyCommandTarget target, float duration) : base(target, duration) { }

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

        validateTween = DOVirtual.DelayedCall(timeScale * invalidDuration, () => input.ValidateInput()).Play();

        return ObservableComplete(timeScale);
    }
}
public class UndeadSleep : UndeadCommand
{
    ICommand resurrection;
    public UndeadSleep(EnemyCommandTarget target, float duration, ICommand resurrection) : base(target, duration)
    {
        this.resurrection = resurrection;
    }

    public override IObservable<Unit> Execute()
    {
        undeadAnim.sleep.Bool = true;
        undeadAnim.die.Fire();
        enemyMap.ResetTile();
        input.Interrupt(resurrection, false);

        return ObservableComplete(); // Don't validate input
    }
}
