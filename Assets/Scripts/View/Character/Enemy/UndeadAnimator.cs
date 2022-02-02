public interface IUndeadAnimator : IEnemyAnimator
{
    MobAnimator.AnimatorTrigger resurrection { get; }
    MobAnimator.AnimatorBool sleep { get; }
}

public class UndeadAnimator : EnemyAnimator, IUndeadAnimator
{
    public AnimatorTrigger resurrection { get; protected set; }
    public AnimatorBool sleep { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        resurrection = new AnimatorTrigger(anim, "Resurrection");
        sleep = new AnimatorBool(anim, "Sleep");
    }
}
