public interface IMagicianAnimator : IEnemyTurnAnimator
{
    MobAnimator.AnimatorTrigger magic { get; }
    MobAnimator.AnimatorBool teleport { get; }
}

public class MagicianAnimator : EnemyTurnAnimator, IMagicianAnimator
{
    public AnimatorTrigger magic { get; protected set; }
    public AnimatorBool teleport { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        magic = new AnimatorTrigger(anim, "Magic");
        teleport = new AnimatorBool(anim, "Teleport");
    }
}
