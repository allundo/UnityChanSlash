using UnityEngine;
using UniRx;
using System;
using DG.Tweening;

public abstract class RabbitCommand : EnemyCommand
{
    protected RabbitAnimator rabbitAnim;

    public RabbitCommand(EnemyCommandTarget target, float duration, float validateTiming = 0.95f) : base(target, duration, validateTiming)
    {
        rabbitAnim = target.anim as RabbitAnimator;
    }
}

public abstract class RabbitAttack : RabbitCommand
{
    protected IAttack jumpAttack;
    protected IAttack somersault;

    public RabbitAttack(EnemyCommandTarget target, float duration) : base(target, duration)
    {
        jumpAttack = target.enemyAttack[0];
        somersault = target.enemyAttack[1];
    }

    protected bool IsPlayerOnTile(Pos pos) => GameManager.Instance.IsOnPlayerTile(pos);

    protected Tween JumpAttack(int distance = 0)
    {
        var seq = DOTween.Sequence().AppendInterval(duration * 0.1f);

        switch (distance)
        {
            case 1:
                map.MoveObjectOn(map.GetForward);
                seq.Append(tweenMove.JumpRelative(map.GetForwardVector(0.8f), 0.65f))
                    .AppendCallback(() => enemyMap.MoveOnEnemy());
                break;

            case 2:
                Pos forward = map.GetForward;
                map.MoveObjectOn(forward);
                seq.Append(tweenMove.JumpRelative(map.GetForwardVector(1.5f), 0.3f))
                    .AppendCallback(() => enemyMap.MoveOnEnemy(forward))
                    .Append(tweenMove.JumpRelative(map.GetBackwardVector(0.7f), 0.35f, 0.5f));
                break;

            case 3:
                map.MoveObjectOn(map.GetJump);
                seq.Append(tweenMove.JumpRelative(map.GetForwardVector(1.8f), 0.3f))
                    .AppendCallback(() => enemyMap.MoveOnEnemy())
                    .Append(tweenMove.JumpRelative(map.GetBackwardVector(0.7f), 0.35f, 0.5f));
                break;


            case 0:
            default:
                seq.Append(tweenMove.JumpRelative(map.GetForwardVector(0.5f), 0.3f))
                    .Append(tweenMove.JumpRelative(map.GetBackwardVector(0.7f), 0.35f, 0.5f));
                break;
        }

        return seq.Append(tweenMove.Jump(map.DestVec3Pos, 0.25f, 0.25f)
            .SetEase(Ease.InQuad))
            .SetUpdate(false)
            .Play();
    }
}

public class RabbitJumpAttack : RabbitAttack
{
    public RabbitJumpAttack(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        rabbitAnim.attack.Fire();
        completeTween = jumpAttack.AttackSequence(duration).Play();
        playingTween = JumpAttack(map.IsForwardMovable ? 1 : 0);
        return true;
    }
}

public class RabbitLongJumpAttack : RabbitAttack
{
    public RabbitLongJumpAttack(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        int distance = 3;

        if (IsPlayerOnTile(map.GetJump))
        {
            if (!map.IsForwardMovable) return false;
            distance = 2;
        }
        else if (IsPlayerOnTile(map.GetForward))
        {
            distance = 0;
        }
        else if (!map.IsJumpable)
        {
            return false;
        }

        rabbitAnim.attack.Fire();
        completeTween = jumpAttack.AttackSequence(duration).Play();
        playingTween = JumpAttack(distance);
        return true;
    }
}
public class RabbitSomersault : RabbitAttack
{
    private Command attack;
    public RabbitSomersault(EnemyCommandTarget target, float duration) : base(target, duration)
    {
        attack = new RabbitJumpAttack(target, duration);
    }

    protected override bool Action()
    {
        Pos backward = map.GetBackward;

        if (!map.IsMovable(backward))
        {
            target.input.Interrupt(attack, false);
            return false;
        }

        rabbitAnim.somersault.Fire();
        completeTween = somersault.AttackSequence(duration).Play();

        map.MoveObjectOn(backward);
        playingTween = DOTween.Sequence()
            .AppendInterval(0.1f * duration)
            .Join(tweenMove.JumpRelative(map.GetBackwardVector(), 0.4f).SetEase(Ease.InQuart))
            .InsertCallback(0.65f * duration, () => enemyMap.MoveOnEnemy(backward))
            .Play();

        return true;
    }
}
public class RabbitWondering : RabbitCommand
{
    public RabbitWondering(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        rabbitAnim.wondering.Bool = true;
        SetOnCompleted(() => rabbitAnim.wondering.Bool = false);
        return true;
    }
}
public class RabbitIcedFall : RabbitCommand
{
    protected float meltFrameTimer;
    public RabbitIcedFall(EnemyCommandTarget target, float framesToMelt, float duration) : base(target, duration)
    {
        meltFrameTimer = Mathf.Min(framesToMelt, duration + 1f) * FRAME_UNIT;
    }

    public override IObservable<Unit> Execute()
    {
        rabbitAnim.speed.Float = 0f;
        rabbitAnim.icedFall.Bool = true;

        playingTween = DOTween.Sequence()
            .AppendCallback(mobReact.OnFall)
            .Append(tweenMove.Jump(map.DestVec3Pos + map.GetBackwardVector(0.2f), 1f, 0f).SetEase(Ease.Linear))
            .AppendCallback(() => mobReact.Damage(0.5f, null, AttackType.Smash))
            .SetUpdate(false)
            .Play();

        completeTween = DOVirtual.DelayedCall(meltFrameTimer, () => mobReact.OnMelt(), false).Play();

        return ObservableComplete();
    }
}

public class RabbitWakeUp : RabbitCommand
{
    protected float wakeUpTiming;
    public RabbitWakeUp(EnemyCommandTarget target, float duration, float wakeUpTiming = 0.5f) : base(target, duration)
    {
        this.wakeUpTiming = wakeUpTiming;
    }

    protected override bool Action()
    {
        playingTween = tweenMove.Jump(map.DestVec3Pos, 0.25f, 0.25f).SetEase(Ease.InQuad).SetDelay(duration * wakeUpTiming).Play();

        completeTween = DOTween.Sequence()
            .InsertCallback(duration * wakeUpTiming, () => rabbitAnim.icedFall.Bool = false)
            .InsertCallback(duration * (1f + wakeUpTiming) * 0.5f, mobReact.OnWakeUp)
            .SetUpdate(false)
            .Play();

        return true;
    }
}
