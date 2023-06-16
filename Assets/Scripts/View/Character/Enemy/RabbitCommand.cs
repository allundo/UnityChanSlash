using UnityEngine;
using UniRx;
using System;
using DG.Tweening;

public abstract class RabbitCommand : EnemyCommand
{
    protected RabbitAnimator rabbitAnim;

    public RabbitCommand(ICommandTarget target, float duration, float validateTiming = 0.95f) : base(target, duration, validateTiming)
    {
        rabbitAnim = target.anim as RabbitAnimator;
    }
}

public abstract class RabbitAttack : RabbitCommand
{
    protected IAttack jumpAttack;
    protected IAttack somersault;

    public RabbitAttack(ICommandTarget target, float duration) : base(target, duration)
    {
        jumpAttack = target.Attack(0);
        somersault = target.Attack(1);
    }

    protected Tween JumpAttack(int distance = 0)
    {
        var seq = DOTween.Sequence().AppendInterval(duration * 0.1f);

        switch (distance)
        {
            case 1:
                mobMap.MoveObjectOn(map.GetForward);
                seq.Append(tweenMove.JumpRelative(mobMap.GetForwardVector(0.8f), 0.65f))
                    .AppendCallback(() => enemyMap.MoveOnEnemy());
                break;

            case 2:
                Pos forward = mobMap.GetForward;
                mobMap.MoveObjectOn(forward);
                seq.Append(tweenMove.JumpRelative(mobMap.GetForwardVector(1.5f), 0.3f))
                    .AppendCallback(() => enemyMap.MoveOnEnemy(forward))
                    .Append(tweenMove.JumpRelative(mobMap.GetBackwardVector(0.7f), 0.35f, 0.5f));
                break;

            case 3:
                mobMap.MoveObjectOn(map.GetJump);
                seq.Append(tweenMove.JumpRelative(mobMap.GetForwardVector(1.8f), 0.3f))
                    .AppendCallback(() => enemyMap.MoveOnEnemy())
                    .Append(tweenMove.JumpRelative(mobMap.GetBackwardVector(0.7f), 0.35f, 0.5f));
                break;


            case 0:
            default:
                seq.Append(tweenMove.JumpRelative(mobMap.GetForwardVector(0.5f), 0.3f))
                    .Append(tweenMove.JumpRelative(mobMap.GetBackwardVector(0.7f), 0.35f, 0.5f));
                break;
        }

        return seq.Append(tweenMove.Jump(mobMap.DestVec3Pos, 0.25f, 0.25f)
            .SetEase(Ease.InQuad))
            .SetUpdate(false)
            .Play();
    }
}

public class RabbitJumpAttack : RabbitAttack
{
    public RabbitJumpAttack(ICommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        rabbitAnim.attack.Fire();
        completeTween = jumpAttack.AttackSequence(duration).Play();
        playingTween = JumpAttack(mobMap.IsForwardMovable ? 1 : 0);
        return true;
    }
}

public class RabbitLongJumpAttack : RabbitAttack
{
    public RabbitLongJumpAttack(ICommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        int distance = 3;

        if (map.IsOnPlayer(map.GetJump))
        {
            if (!mobMap.IsForwardMovable) return false;
            distance = 2;
        }
        else if (map.IsOnPlayer(map.GetForward))
        {
            distance = 0;
        }
        else if (!mobMap.IsJumpable)
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
    public RabbitSomersault(ICommandTarget target, float duration) : base(target, duration)
    {
        attack = new RabbitJumpAttack(target, duration);
    }

    protected override bool Action()
    {
        Pos backward = mobMap.GetBackward;

        if (!mobMap.IsMovable(backward))
        {
            target.input.Interrupt(attack, false);
            return false;
        }

        rabbitAnim.somersault.Fire();
        completeTween = somersault.AttackSequence(duration).Play();

        mobMap.MoveObjectOn(backward);
        playingTween = DOTween.Sequence()
            .AppendInterval(0.1f * duration)
            .Append(tweenMove.JumpRelative(mobMap.GetBackwardVector(), 0.4f).SetEase(Ease.InQuart))
            .AppendInterval(duration * 0.5f)
            .InsertCallback(0.65f * duration, () => enemyMap.MoveOnEnemy(backward))
            .Play();

        return true;
    }
}
public class RabbitWondering : RabbitCommand
{
    public RabbitWondering(ICommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        rabbitAnim.wondering.Bool = true;
        SetOnCompleted(() => rabbitAnim.wondering.Bool = false);
        return true;
    }
}
public class RabbitIcedFall : RabbitCommand, IIcedCommand
{
    protected float meltTime;
    public float framesToMelt { get; protected set; }

    public RabbitIcedFall(ICommandTarget target, float framesToMelt, float duration) : base(target, duration)
    {
        this.framesToMelt = framesToMelt;
        meltTime = Mathf.Min(framesToMelt, duration + 1f) * FRAME_UNIT;
    }

    public override IObservable<Unit> Execute()
    {
        rabbitAnim.speed.Float = 0f;
        rabbitAnim.icedFall.Bool = true;

        mobReact.Iced(framesToMelt);

        playingTween = DOTween.Sequence()
            .AppendCallback(mobReact.OnFall)
            .Append(tweenMove.Jump(mobMap.DestVec3Pos + map.GetBackwardVector(0.2f), 1f, 0f).SetEase(Ease.Linear))
            .AppendCallback(() => mobReact.Damage(5f, map.dir, AttackType.Smash))
            .SetUpdate(false)
            .Play();

        completeTween = DOVirtual.DelayedCall(meltTime, () => mobReact.Melt(), false).Play();

        return ObservableComplete();
    }
}

public class RabbitWakeUp : RabbitCommand
{
    protected float wakeUpTiming;
    public RabbitWakeUp(ICommandTarget target, float duration, float wakeUpTiming = 0.5f) : base(target, duration)
    {
        this.wakeUpTiming = wakeUpTiming;
    }

    protected override bool Action()
    {
        playingTween = tweenMove.Jump(mobMap.DestVec3Pos, 0.25f, 0.25f).SetEase(Ease.InQuad).SetDelay(duration * wakeUpTiming).Play();

        completeTween = DOTween.Sequence()
            .InsertCallback(duration * wakeUpTiming, () => rabbitAnim.icedFall.Bool = false)
            .InsertCallback(duration * (1f + wakeUpTiming) * 0.5f, mobReact.OnWakeUp)
            .SetUpdate(false)
            .Play();

        return true;
    }
}
