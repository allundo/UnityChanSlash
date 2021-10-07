using UniRx;
using System;
using DG.Tweening;

public abstract class EnemyCommand : Command
{
    protected EnemyAnimator enemyAnim;

    public EnemyCommand(EnemyCommandTarget target, float duration, float validateTiming = 0.5f) : base(target, duration, validateTiming)
    {
        enemyAnim = anim as EnemyAnimator;
    }
}

public abstract class EnemyMove : EnemyCommand
{
    public EnemyMove(EnemyCommandTarget target, float duration) : base(target, duration, 0.95f) { }

    protected abstract bool IsMovable { get; }
    protected abstract Pos GetDest { get; }
    protected Pos prevPos = new Pos();

    protected void SetSpeed()
    {
        enemyAnim.speed.Float = Speed;
    }

    protected void ResetSpeed()
    {
        enemyAnim.speed.Float = 0.0f;
    }

    public override void Cancel()
    {
        base.Cancel();
        map.MoveOnCharacter(prevPos);
    }

    protected override IObservable<bool> Execute(IObservable<bool> execObservable)
    {
        if (!IsMovable)
        {
            Cancel();
            return Observable.Return(false);
        }

        prevPos = map.CurrentPos;
        var destPos = map.MoveOnCharacter(GetDest);

        SetSpeed();
        playingTween = tweenMove.GetLinearMove(map.WorldPos(destPos)).OnComplete(ResetSpeed).Play();

        return execObservable;
    }
}

public class EnemyForward : EnemyMove
{
    public EnemyForward(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override bool IsMovable => map.IsForwardMovable;
    protected override Pos GetDest => map.GetForward;
    public override float Speed => TILE_UNIT / duration;
}

public class EnemyTurnL : EnemyCommand
{
    public EnemyTurnL(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override void Action()
    {
        playingTween = tweenMove.TurnL.Play();
        map.TurnLeft();
    }
}

public class EnemyTurnR : EnemyCommand
{
    public EnemyTurnR(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override void Action()
    {
        playingTween = tweenMove.TurnR.Play();
        map.TurnRight();
    }
}

public class EnemyAttack : EnemyCommand
{
    private MobAttack enemyAttack;
    public EnemyAttack(EnemyCommandTarget target, float duration) : base(target, duration)
    {
        enemyAttack = target.enemyAttack;
    }

    protected override void Action()
    {
        enemyAnim.attack.Fire();
        playingTween = enemyAttack.AttackSequence(duration).Play();
    }
}
