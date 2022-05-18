public class ShieldEnemyTurnL : EnemyTurnL
{
    public ShieldEnemyTurnL(ICommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        (anim as ShieldEnemyAnimator).turnL.Fire();
        return base.Action();
    }
}

public class ShieldEnemyTurnR : EnemyTurnR
{
    public ShieldEnemyTurnR(ICommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        (anim as ShieldEnemyAnimator).turnR.Fire();
        return base.Action();
    }
}
