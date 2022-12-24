using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;
using DG.Tweening;

public abstract class EnemyCommand : MobCommand
{
    protected static List<Tween> resetTweens = new List<Tween>();
    public static void ClearResetTweens()
    {
        resetTweens.ForEach(tween => tween.Kill());
        resetTweens.Clear();
    }

    protected IEnemyReactor enemyReact;
    protected IEnemyAnimator enemyAnim;
    protected EnemyMapUtil enemyMap;

    public EnemyCommand(ICommandTarget target, float duration, float validateTiming = 0.5f) : base(target, duration, validateTiming)
    {
        enemyReact = react as IEnemyReactor;
        enemyAnim = target.anim as IEnemyAnimator;
        enemyMap = target.map as EnemyMapUtil;
    }
}

public class EnemyIdle : EnemyCommand
{
    public EnemyIdle(ICommandTarget target, float duration) : base(target, duration) { }
    protected override bool Action() => true;
}

public abstract class EnemyMove : EnemyCommand
{
    protected Tween speedTween;
    protected Tween resetTween;
    public EnemyMove(ICommandTarget target, float duration) : base(target, duration, 0.95f) { }

    protected abstract bool IsMovable { get; }
    protected abstract Pos GetDest { get; }

    public override void Cancel()
    {
        speedTween?.Kill();
        enemyAnim.speed.Float = 0f;

        playingTween?.Kill();
        validateTween?.Kill();
        input.ValidateInput();
        onCompleted.Clear();
    }

    protected virtual void SetSpeed()
    {
        resetTween?.Kill();
        resetTweens.Remove(resetTween);
        speedTween = DOTween.To(() => enemyAnim.speed.Float, value => enemyAnim.speed.Float = value, Speed, 0.5f).Play();
    }

    protected virtual void ResetSpeed()
    {
        speedTween?.Kill();
        resetTween = DOTween.To(() => enemyAnim.speed.Float, value => enemyAnim.speed.Float = value, 0f, 0.25f);
        resetTweens.Add(resetTween);
        resetTween.OnComplete(() => resetTweens.Remove(resetTween)).Play();
    }

    protected Tween LinearMove(Pos destPos)
    {
        mobMap.MoveObjectOn(destPos);

        SetSpeed();

        return DOTween.Sequence()
            .Join(tweenMove.Move(destPos))
            .Join(tweenMove.DelayedCall(0.51f, () => enemyMap.MoveOnEnemy()))
            .AppendCallback(ResetSpeed)
            .Play();
    }

    protected override bool Action()
    {
        if (!IsMovable)
        {
            return false;
        }

        playingTween = LinearMove(GetDest);

        return true;
    }
}

public class EnemyForward : EnemyMove
{
    public EnemyForward(ICommandTarget target, float duration) : base(target, duration) { }

    protected override bool IsMovable => mobMap.IsForwardMovable;
    protected override Pos GetDest => mobMap.GetForward;
    protected override float Speed => TILE_UNIT / duration;
}

public class EnemyTurnL : EnemyCommand
{
    public EnemyTurnL(ICommandTarget target, float duration) : base(target, duration, 0.95f) { }

    protected override bool Action()
    {
        mobMap.TurnLeft();
        playingTween = tweenMove.TurnToDir().Play();
        return true;
    }
}

public class EnemyTurnR : EnemyCommand
{
    public EnemyTurnR(ICommandTarget target, float duration) : base(target, duration, 0.95f) { }

    protected override bool Action()
    {
        mobMap.TurnRight();
        playingTween = tweenMove.TurnToDir().Play();
        return true;
    }
}

public class EnemyAttack : EnemyCommand
{
    protected IAttack enemyAttack;

    public EnemyAttack(ICommandTarget target, float duration) : base(target, duration, 0.95f)
    {
        enemyAttack = target.Attack(0);
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
    public EnemyDoubleAttack(ICommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        playingTween = DOTween.Sequence()
            .AppendCallback(enemyAnim.attack.Fire)
            .AppendCallback(() => completeTween = enemyAttack.AttackSequence(duration).Play())
            .AppendInterval(duration * 0.12f)
            .AppendCallback(enemyAnim.attack.Fire)
            .AppendCallback(() => completeTween = enemyAttack.AttackSequence(duration).Play())
            .Play();

        return true;
    }
}

public class EnemyFire : EnemyAttack
{
    protected BulletType type;
    public EnemyFire(ICommandTarget target, float duration, BulletType type = BulletType.FireBall) : base(target, duration)
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
    public EnemyDie(ICommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        var attacker = enemyReact.lastAttacker as IGetExp;
        if (attacker != null)
        {
            attacker.AddExp(enemyReact.ExpObtain);
            GameInfo.Instance.defeatCount++;
        }

        anim.die.Bool = true;
        react.OnDie();

        return ExecOnCompleted(() => mobReact.OnDisappear()); // Don't validate input.
    }
}

public class EnemySummoned : EnemyCommand
{
    public EnemySummoned(ICommandTarget target, float duration = 120f) : base(target, duration)
    { }

    protected override bool Action()
    {
        enemyReact.OnSummoned();
        completeTween = DOVirtual.DelayedCall(Mathf.Min(duration, 1.5f), enemyReact.OnTeleportEnd, false).Play();
        return true;
    }
}
