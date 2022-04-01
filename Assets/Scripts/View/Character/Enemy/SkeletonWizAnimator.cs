public class SkeletonWizAnimator : EnemyTurnAnimator, IUndeadAnimator, IMagicianAnimator
{
    public AnimatorTrigger magic { get; protected set; }
    public AnimatorTrigger resurrection { get; protected set; }
    public AnimatorBool sleep { get; protected set; }
    public AnimatorBool teleport { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        magic = new AnimatorTrigger(anim, "Magic");
        resurrection = new AnimatorTrigger(anim, "Resurrection");
        sleep = new AnimatorBool(anim, "Sleep");
        teleport = new AnimatorBool(anim, "Teleport");
    }
}
