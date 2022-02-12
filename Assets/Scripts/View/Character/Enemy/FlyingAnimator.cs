public class FlyingAnimator : EnemyTurnAnimator
{
    public AnimatorTrigger leaveF { get; protected set; }
    public AnimatorTrigger leaveR { get; protected set; }
    public AnimatorTrigger leaveL { get; protected set; }
    public AnimatorBool icedFall { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        leaveF = new AnimatorTrigger(anim, "LeaveF");
        leaveR = new AnimatorTrigger(anim, "LeaveR");
        leaveL = new AnimatorTrigger(anim, "LeaveL");
        icedFall = new AnimatorBool(anim, "IcedFall");
    }
}
