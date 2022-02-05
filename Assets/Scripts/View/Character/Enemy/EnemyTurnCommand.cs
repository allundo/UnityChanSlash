using DG.Tweening;

public abstract class EnemyTurnCommand : EnemyCommand
{
    protected EnemyTurnAnimator turnAnim;

    public EnemyTurnCommand(EnemyCommandTarget target, float duration, float validateTiming = 0.95f) : base(target, duration, validateTiming)
    {
        turnAnim = target.anim as EnemyTurnAnimator;
    }
}

public class EnemyTurnAnimL : EnemyTurnCommand
{
    public EnemyTurnAnimL(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        playingTween = tweenMove.TurnL.Play();
        turnAnim.turnL.Fire();
        map.TurnLeft();
        return true;
    }
}

public class EnemyTurnAnimR : EnemyTurnCommand
{
    public EnemyTurnAnimR(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        playingTween = tweenMove.TurnR.Play();
        turnAnim.turnR.Fire();
        map.TurnRight();
        return true;
    }
}
