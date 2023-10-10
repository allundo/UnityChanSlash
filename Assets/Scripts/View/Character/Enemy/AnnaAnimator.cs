public class AnnaAnimator : ShieldEnemyAnimator
{
    public AnimatorTrigger slash { get; protected set; }
    public AnimatorFloat speedLR { get; protected set; }
    public AnimatorBool jump { get; protected set; }
    public AnimatorBool jumpSlash { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        slash = new AnimatorTrigger(anim, "Slash");
        speedLR = new AnimatorFloat(anim, "SpeedLR");
        jump = new AnimatorBool(anim, "Jump");
        jumpSlash = new AnimatorBool(anim, "JumpSlash");
    }
}