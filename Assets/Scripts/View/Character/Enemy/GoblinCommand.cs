using DG.Tweening;

public class GoblinTurnL : EnemyTurnL
{
    public GoblinTurnL(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        (anim as ShieldEnemyAnimator).turnL.Fire();
        return base.Action();
    }
}

public class GoblinTurnR : EnemyTurnR
{
    public GoblinTurnR(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        (anim as ShieldEnemyAnimator).turnR.Fire();
        return base.Action();
    }
}

public class GoblinGuard : ShieldCommand
{
    public GoblinGuard(CommandTarget target, float duration) : base(target, duration)
    { }

    protected override bool Action()
    {
        (anim as ShieldEnemyAnimator).guard.Bool = true;
        completeTween = tweenMove.FinallyCall(() => (anim as ShieldEnemyAnimator).guard.Bool = false).Play();
        return true;
    }
}