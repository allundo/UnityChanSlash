public class EnemyAnimator : MobAnimator
{
    public AnimatorTrigger attack { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        attack = new AnimatorTrigger(anim, "Attack");
    }

    protected override void Update()
    {
        // None
    }
}
