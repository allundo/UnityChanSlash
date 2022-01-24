using DG.Tweening;

public abstract class RabbitCommand : Command
{
    protected RabbitAnimator rabbitAnim;
    protected EnemyMapUtil enemyMap;

    public RabbitCommand(EnemyCommandTarget target, float duration, float validateTiming = 0.95f) : base(target, duration, validateTiming)
    {
        rabbitAnim = target.anim as RabbitAnimator;
        enemyMap = target.map as EnemyMapUtil;
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
                seq.Append(tweenMove.Jump(map.GetForwardVector(0.8f), 0.65f))
                    .AppendCallback(() => enemyMap.MoveOnEnemy());
                break;

            case 2:
                Pos forward = map.GetForward;
                map.MoveObjectOn(forward);
                seq.Append(tweenMove.Jump(map.GetForwardVector(1.5f), 0.3f))
                    .AppendCallback(() => enemyMap.MoveOnEnemy(forward))
                    .Append(tweenMove.Jump(map.GetBackwardVector(0.7f), 0.35f, 0.5f));
                break;

            case 3:
                map.MoveObjectOn(map.GetJump);
                seq.Append(tweenMove.Jump(map.GetForwardVector(1.8f), 0.3f))
                    .AppendCallback(() => enemyMap.MoveOnEnemy())
                    .Append(tweenMove.Jump(map.GetBackwardVector(0.7f), 0.35f, 0.5f));
                break;


            case 0:
            default:
                seq.Append(tweenMove.Jump(map.GetForwardVector(0.5f), 0.3f))
                    .Append(tweenMove.Jump(map.GetBackwardVector(0.7f), 0.35f, 0.5f));
                break;
        }

        return seq.Append(tweenMove.Jump(map.GetForwardVector(0.2f), 0.25f, 0.25f).SetEase(Ease.InQuad)).Play();
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
            .Join(tweenMove.Jump(map.GetBackwardVector(), 0.4f).SetEase(Ease.InQuart))
            .AppendInterval(0.15f * duration)
            .AppendCallback(() => enemyMap.MoveOnEnemy(backward))
            .AppendInterval(0.5f * duration)
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

public class RabbitTurnL : RabbitCommand
{
    public RabbitTurnL(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        playingTween = tweenMove.TurnL.Play();
        rabbitAnim.turnL.Fire();
        map.TurnLeft();
        return true;
    }
}

public class RabbitTurnR : RabbitCommand
{
    public RabbitTurnR(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        playingTween = tweenMove.TurnR.Play();
        rabbitAnim.turnR.Fire();
        map.TurnRight();
        return true;
    }
}
