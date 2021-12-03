public class EnemyAnimator : MobAnimator
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
