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

public class EnemyIdle : EnemyCommand
{
    public EnemyIdle(EnemyCommandTarget target, float duration) : base(target, duration) { }
    protected override bool Action() => true;
}

public abstract class EnemyMove : EnemyCommand
{
    public EnemyMove(EnemyCommandTarget target, float duration) : base(target, duration, 0.95f) { }

    protected abstract bool IsMovable { get; }
    protected abstract Pos GetDest { get; }

    protected virtual void SetSpeed()
    {
        enemyAnim.speed.Float = Speed;
    }

    protected virtual void ResetSpeed()
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
    protected override float Speed => TILE_UNIT / duration;
}

public class EnemyTurnL : EnemyCommand
{
    public EnemyTurnL(EnemyCommandTarget target, float duration) : base(target, duration, 0.95f) { }

    protected override bool Action()
    {
        map.TurnLeft();
        playingTween = tweenMove.TurnToDir.Play();
        return true;
    }
}

public class EnemyTurnR : EnemyCommand
{
    public EnemyTurnR(EnemyCommandTarget target, float duration) : base(target, duration, 0.95f) { }

    protected override bool Action()
    {
        map.TurnRight();
        playingTween = tweenMove.TurnToDir.Play();
        return true;
    }
}

public class EnemyAttack : EnemyCommand
{
    protected IAttack enemyAttack;

    public EnemyAttack(EnemyCommandTarget target, float duration) : base(target, duration, 0.95f)
    {
        enemyAttack = target.enemyAttack[0];
    }

    protected override bool Action()
    {
        enemyAnim.attack.Fire();
        completeTween = enemyAttack.AttackSequence(duration).Play();
        return true;
    }
}

public class EnemyDoubleAttack : EnemyAttack
{
    public EnemyDoubleAttack(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        completeTween = DOTween.Sequence()
            .AppendCallback(enemyAnim.attack.Fire)
            .Join(enemyAttack.AttackSequence(duration))
            .AppendInterval(duration * 0.12f)
            .AppendCallback(enemyAnim.attack.Fire)
            .Join(enemyAttack.AttackSequence(duration))
            .Play();

        return true;
    }
}

public class EnemyFire : EnemyAttack
{
    protected BulletType type;
    public EnemyFire(EnemyCommandTarget target, float duration, BulletType type = BulletType.FireBall) : base(target, duration)
    {
        this.type = type;
    }

    protected override bool Action()
    {
        enemyAnim.fire.Fire();
        playingTween = target.magic?.MagicSequence(type, duration)?.Play();
        return true;
    }
}

public class EnemyDie : EnemyCommand
{
    public EnemyDie(EnemyCommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        anim.die.Fire();
        react.OnDie();

        return ExecOnCompleted(() => react.FadeOutToDead()); // Don't validate input.
    }
}
