public class WitchDoubleAnimator : MobAnimator
{
    public AnimatorTrigger jumpOver { get; protected set; }
    public AnimatorTrigger backStep { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        jumpOver = new AnimatorTrigger(anim, "Jump");
        backStep = new AnimatorTrigger(anim, "BackStep");
    }
}
