public class SkeletonSoldierAnimator : ShieldEnemyAnimator, IUndeadAnimator
{
    public AnimatorTrigger resurrection { get; protected set; }
    public AnimatorBool sleep { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        resurrection = new TriggerEx(triggers, anim, "Resurrection", 0);
        sleep = new AnimatorBool(anim, "Sleep");
    }
}
