public interface IEnemyAnimator
{
    MobAnimator.AnimatorFloat speed { get; }
    MobAnimator.AnimatorTrigger die { get; }
    MobAnimator.AnimatorTrigger attack { get; }
    MobAnimator.AnimatorTrigger fire { get; }
}

public class EnemyAnimator : MobAnimator, IEnemyAnimator
{
    public AnimatorTrigger attack { get; protected set; }
    public AnimatorTrigger fire { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        attack = new AnimatorTrigger(anim, "Attack");
        fire = new AnimatorTrigger(anim, "Fire");
    }
}
