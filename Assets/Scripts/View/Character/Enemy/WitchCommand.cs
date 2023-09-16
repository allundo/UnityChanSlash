using UnityEngine;
using UniRx;
using System;
using DG.Tweening;

public class WitchCommand : EnemyTurnCommand
{
    protected WitchAnimator witchAnim;
    protected WitchReactor witchReact;
    protected MagicAndDouble magicAndDouble;

    public WitchCommand(CommandTarget target, float duration, float validateTiming = 0.95f) : base(target, duration, validateTiming)
    {
        witchAnim = target.anim as WitchAnimator;
        witchReact = target.react as WitchReactor;
        magicAndDouble = target.magic as MagicAndDouble;
    }
}

public class WitchTargetAttack : WitchCommand
{
    protected IAttack targetAttack;

    public WitchTargetAttack(CommandTarget target, float duration) : base(target, duration)
    {
        targetAttack = target.Attack(1);
    }

    protected override bool Action()
    {
        if (!witchReact.Appear()) return false;

        witchAnim.targetAttack.Fire();
        completeTween = targetAttack.AttackSequence(duration).Play();

        return true;
    }
}

public class WitchJumpOver : WitchCommand
{
    public WitchJumpOver(CommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        if (!enemyMap.IsJumpable) return false;

        witchReact.Appear();
        witchAnim.jump.Fire();

        Pos destPos = enemyMap.GetJump;

        enemyMap.MoveObjectOn(destPos);
        enemyMap.TurnBack();

        playingTween = DOTween.Sequence()
            .Join(tweenMove.Jump(destPos, 1f, 1.6f))
            .Join(tweenMove.TurnLB.SetEase(Ease.Linear))
            .InsertCallback(0.4f * duration, () => enemyMap.MoveOnEnemy())
            .InsertCallback(0.8f * duration, () => enemyMap.MoveOnEnemy())
            .SetUpdate(false)
            .Play();

        return true;
    }
}
public class WitchJumpOverAttack : WitchJumpOver
{
    protected ICommand doubleAttack;

    public WitchJumpOverAttack(CommandTarget target, float duration, ICommand doubleAttack) : base(target, duration)
    {
        this.doubleAttack = doubleAttack;
    }

    public override IObservable<Unit> Execute()
    {
        if (!enemyMap.IsJumpable)
        {
            // Cancel attack
            target.validate.OnNext(false);
            return Observable.Empty(Unit.Default);
        }

        magicAndDouble.backStepWitchLauncher.Fire();
        base.Action();

        target.interrupt.OnNext(Data(doubleAttack));
        return ObservableComplete();
    }
}
public class WitchBackStep : WitchCommand
{
    public WitchBackStep(CommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        if (!enemyMap.IsBackwardMovable) return false;

        witchReact.Appear();
        witchAnim.backStep.Fire();

        Pos destPos = map.GetBackward;

        enemyMap.MoveObjectOn(destPos);

        playingTween = DOTween.Sequence()
            .Join(tweenMove.Jump(destPos, 1f, 1f))
            .InsertCallback(0.51f * duration, () => enemyMap.MoveOnEnemy())
            .SetUpdate(false)
            .Play();

        return true;
    }
}

public class WitchBackStepAttack : WitchBackStep
{
    protected ICommand doubleAttack;

    public WitchBackStepAttack(CommandTarget target, float duration, ICommand doubleAttack) : base(target, duration)
    {
        this.doubleAttack = doubleAttack;
    }

    public override IObservable<Unit> Execute()
    {
        if (!enemyMap.IsBackwardMovable)
        {
            // Cancel attack
            target.validate.OnNext(false);
            return Observable.Empty(Unit.Default);
        }

        magicAndDouble.jumpOverWitchLauncher.Fire();
        base.Action();

        target.interrupt.OnNext(Data(doubleAttack));
        return ObservableComplete();
    }
}

public class WitchTripleFire : WitchCommand
{
    public WitchTripleFire(CommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        float interval = FRAME_UNIT * 10f;
        float fireDuration = duration - interval * 2;

        witchReact.Appear();
        witchAnim.fire.Fire();

        ILauncher fire = target.magic.launcher[MagicType.FireBall];
        playingTween = DOTween.Sequence()
            .Join(fire.FireSequence(fireDuration))
            .AppendInterval(interval)
            .Join(fire.FireSequence(fireDuration))
            .AppendInterval(interval)
            .Join(fire.FireSequence(fireDuration))
            .SetUpdate(false)
            .Play();

        return true;
    }
}

public class WitchDoubleIce : WitchCommand
{
    public WitchDoubleIce(CommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        float interval = FRAME_UNIT * 15f;
        float fireDuration = duration - interval;

        witchReact.Appear();
        witchAnim.fire.Fire();

        ILauncher ice = target.magic.launcher[MagicType.IceBullet];

        playingTween = DOTween.Sequence()
            .Join(ice.FireSequence(fireDuration))
            .AppendInterval(interval)
            .Join(ice.FireSequence(fireDuration))
            .SetUpdate(false)
            .Play();

        return true;
    }
}

public class WitchLaser : WitchCommand
{
    public WitchLaser(CommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        witchReact.Appear();
        witchReact.OnLaserStart();
        witchAnim.magic.Fire();

        ILauncher laser = target.magic.launcher[MagicType.LightLaser];
        playingTween = laser.FireSequence(2f).Play();

        return true;
    }
}

public class WitchSummonMonster : WitchCommand
{
    public WitchSummonMonster(CommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        if (witchReact.IsSummoning) return false;

        witchReact.Appear();
        witchReact.OnSummonStart();
        witchAnim.summon.Fire();
        playingTween = tweenMove.DelayedCall(0.8f, witchReact.Summon).Play();
        return true;
    }
}

public class WitchDoubleAttackLaunch : FlyingAttack
{
    protected ICommand doubleAttackStart;
    public WitchDoubleAttackLaunch(CommandTarget target, float duration) : base(target, duration)
    {
        doubleAttackStart = new GhostAttackStart(target, duration * 0.5f);
    }

    public override IObservable<Unit> Execute()
    {
        if (!IsForwardMovable)
        {
            // Cancel attack
            target.validate.OnNext(false);
            return Observable.Empty(Unit.Default);
        }

        (anim as WitchAnimator).attackStart.Fire();
        target.interrupt.OnNext(Data(doubleAttackStart));
        return ObservableComplete();
    }
}

public class WitchSleep : UndeadSleep
{
    public WitchSleep(CommandTarget target, float duration = 300f, ICommand resurrection = null)
        : base(target, duration, resurrection ?? new WitchResurrection(target))
    { }

    public override IObservable<Unit> Execute()
    {
        var pit = map.OnTile as Pit;

        if (pit != null)
        {
            completeTween = DOTween.Sequence()
                .Join(tweenMove.Move(map.DestVec3Pos - Vector3.up * TILE_UNIT, 0.2f, Ease.InCubic))
                .InsertCallback(0.4f, pit.Drop)
                .Play();
        }

        return base.Execute(); // Don't validate input
    }

}
public class WitchQuickSleep : UndeadQuickSleep
{
    public WitchQuickSleep(CommandTarget target, float duration = 15f, ICommand resurrection = null)
        : base(target, duration, resurrection ?? new WitchResurrection(target))
    { }

    public override IObservable<Unit> Execute()
    {
        var pit = map.OnTile as Pit;

        if (pit != null)
        {
            target.transform.position -= Vector3.up * TILE_UNIT;
        }

        return base.Execute(); // Don't validate input
    }
}

public class WitchResurrection : Resurrection
{
    protected ICommand teleport;

    public WitchResurrection(CommandTarget target) : base(target)
    {
        teleport = new MagicianTeleport(target, 84f);
    }

    public override IObservable<Unit> Execute()
    {
        ITile tile = map.OnTile;
        Pos destPos = enemyMap.SearchSpaceNearBy(PlayerInfo.Instance.Pos); // Check if teleport is valid.

        bool isInsideClosedDoor = !tile.IsEnterable();

        // Reserve resurrection again if player is on or teleport is invalid.
        if (tile.IsCharacterOn || isInsideClosedDoor && destPos.IsNull)
        {
            target.interrupt.OnNext(Data(this));
        }
        else
        {
            undeadAnim.resurrection.Fire();
            undeadAnim.die.Bool = undeadAnim.sleep.Bool = false;
            undeadReact.OnResurrection();
            map.SetObjectOn();
            enemyMap.SetOnEnemy();

            if (isInsideClosedDoor)
            {
                target.interrupt.OnNext(Data(teleport));
            }
            else if (map.DestVec.magnitude > 0f)
            {
                target.interrupt.OnNext(Data(startMoving));
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

