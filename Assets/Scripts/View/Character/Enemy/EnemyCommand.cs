using UniRx;
using System;
using DG.Tweening;

public abstract class EnemyCommand : Command
{
    protected IEnemyAnimator enemyAnim;
    protected EnemyMapUtil enemyMap;

    public EnemyCommand(EnemyCommandTarget target, float duration, float validateTiming = 0.5f) : base(target, duration, validateTiming)
    {
        enemyAnim = target.anim as IEnemyAnimator;
        enemyMap = target.map as EnemyMapUtil;
    }
}

public abstract class EnemyMove : EnemyCommand
{
    public EnemyMove(EnemyCommandTarget target, float duration) : base(target, duration, 0.95f) { }

    protected abstract bool IsMovable { get; }
    protected abstract Pos GetDest { get; }

    protected void SetSpeed()
    {
        enemyAnim.speed.Float = Speed;
    }

    protected void ResetSpeed()
    {
        enemyAnim.speed.Float = 0.0f;
    }

    protected Tween LinearMove(Pos destPos)
    {
        map.MoveObjectOn(destPos);

        return DOTween.Sequence()
            .Join(tweenMove.Move(destPos))
            .Join(tweenMove.DelayedCall(0.51f, () => enemyMap.MoveOnEnemy()))
            .Play();
    }

    protected override bool Action()
    {
        if (!IsMovable)
        {
            return false;
        }

        playingTween = LinearMove(GetDest);
        SetSpeed();
        completeTween = tweenMove.FinallyCall(ResetSpeed).Play();

        return true;
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
    public EnemyTurnL(EnemyCommandTarget target, float duration) : base(target, duration, 0.95f) { }

    protected override bool Action()
    {
        playingTween = tweenMove.TurnL.Play();
        map.TurnLeft();
        return true;
    }
}

public class EnemyTurnR : EnemyCommand
{
    public EnemyTurnR(EnemyCommandTarget target, float duration) : base(target, duration, 0.95f) { }

    protected override bool Action()
    {
        playingTween = tweenMove.TurnR.Play();
        map.TurnRight();
        return true;
    }
}

public class EnemyAttack : EnemyCommand
{
    protected IAttack enemyAttack;
    protected IAttack enemyFire;

    public EnemyAttack(EnemyCommandTarget target, float duration) : base(target, duration, 0.95f)
    {
        enemyAttack = target.enemyAttack;
        enemyFire = target.fire;
    }

    protected override bool Action()
    {
        enemyAnim.attack.Fire();
        completeTween = enemyAttack.AttackSequence(duration).Play();
        return true;
    }
}

public class EnemyFire : EnemyAttack
{
    public EnemyFire(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        enemyAnim.fire.Fire();
        playingTween = enemyFire?.AttackSequence(duration)?.Play();
        return true;
    }
}

public class EnemyDie : EnemyCommand
{
    public EnemyDie(EnemyCommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        enemyMap.ResetTile();
        anim.die.Fire();
        react.OnDie();

        return ExecOnCompleted(() => react.FadeOutOnDead()); // Don't validate input.
    }
}
