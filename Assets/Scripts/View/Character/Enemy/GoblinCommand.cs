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
