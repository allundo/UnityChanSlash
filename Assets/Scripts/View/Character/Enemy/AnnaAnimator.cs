public class AnnaAnimator : ShieldEnemyAnimator
{
    public AnimatorTrigger slash { get; protected set; }
    public AnimatorBool jump { get; protected set; }
    public AnimatorFloat speedLR { get; protected set; }
    public AnimatorBool speedCommand { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        slash = new AnimatorTrigger(anim, "Slash");
        jump = new AnimatorBool(anim, "Jump");
        speedCommand = new AnimatorBool(anim, "SpeedCommand");
        speedLR = new AnimatorFloat(anim, "SpeedLR");
    }
}
