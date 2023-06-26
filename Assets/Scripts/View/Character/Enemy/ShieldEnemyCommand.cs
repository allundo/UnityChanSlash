public class ShieldEnemyTurnL : EnemyTurnL
{
    public ShieldEnemyTurnL(CommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        (anim as ShieldEnemyAnimator).turnL.Fire();
        return base.Action();
    }
}

public class ShieldEnemyTurnR : EnemyTurnR
{
    public ShieldEnemyTurnR(CommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        (anim as ShieldEnemyAnimator).turnR.Fire();
        return base.Action();
    }
}
