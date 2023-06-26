using DG.Tweening;

public abstract class EnemyTurnCommand : EnemyCommand
{
    protected EnemyTurnAnimator turnAnim;

    public EnemyTurnCommand(CommandTarget target, float duration, float validateTiming = 0.95f) : base(target, duration, validateTiming)
    {
        turnAnim = target.anim as EnemyTurnAnimator;
    }
}

public class EnemyTurnAnimL : EnemyTurnCommand
{
    public EnemyTurnAnimL(CommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        turnAnim.turnL.Fire();
        map.TurnLeft();
        playingTween = tweenMove.TurnToDir().Play();
        return true;
    }
}

public class EnemyTurnAnimR : EnemyTurnCommand
{
    public EnemyTurnAnimR(CommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        turnAnim.turnR.Fire();
        map.TurnRight();
        playingTween = tweenMove.TurnToDir().Play();
        return true;
    }
}
