public class WitchAnimator : FlyingAnimator, IUndeadAnimator
{
    public AnimatorTrigger backStep { get; protected set; }
    public AnimatorTrigger targetAttack { get; protected set; }
    public AnimatorTrigger magic { get; protected set; }
    public AnimatorTrigger resurrection { get; protected set; }
    public AnimatorBool sleep { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        backStep = new AnimatorTrigger(anim, "BackStep");
        targetAttack = new AnimatorTrigger(anim, "TargetAttack");
        magic = new AnimatorTrigger(anim, "Magic");
        resurrection = new AnimatorTrigger(anim, "Resurrection");
        sleep = new AnimatorBool(anim, "Sleep");
    }
}
