using DG.Tweening;

public abstract class EnemyTurnCommand : EnemyCommand
{
    protected EnemyTurnAnimator turnAnim;

    public EnemyTurnCommand(ICommandTarget target, float duration, float validateTiming = 0.95f) : base(target, duration, validateTiming)
    {
        turnAnim = target.anim as EnemyTurnAnimator;
    }
}

public class EnemyTurnAnimL : EnemyTurnCommand
{
    public EnemyTurnAnimL(ICommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        turnAnim.turnL.Fire();
        map.TurnLeft();
        playingTween = tweenMove.TurnToDir.Play();
        return true;
    }
}

public class EnemyTurnAnimR : EnemyTurnCommand
{
    public EnemyTurnAnimR(ICommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        turnAnim.turnR.Fire();
        map.TurnRight();
        playingTween = tweenMove.TurnToDir.Play();
        return true;
    }
}
