public class ShieldEnemyAnimator : ShieldAnimator, IEnemyAnimator
{
    public AnimatorTrigger attack { get; protected set; }
    public AnimatorTrigger fire { get; protected set; }
    public AnimatorBool fighting { get; protected set; }
    public TriggerEx turnL { get; protected set; }
    public TriggerEx turnR { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        attack = new TriggerEx(anim, "Attack");
        fire = new TriggerEx(anim, "Fire");
        fighting = new AnimatorBool(anim, "Fighting");
        turnL = new TriggerEx(anim, "TurnL");
        turnR = new TriggerEx(anim, "TurnR");
    }
}
