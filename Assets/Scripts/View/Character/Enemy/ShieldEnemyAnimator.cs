public class ShieldEnemyAnimator : ShieldAnimator, IEnemyAnimator
{
    public MobAnimator.AnimatorTrigger attack { get; protected set; }
    public MobAnimator.AnimatorTrigger fire { get; protected set; }
    public TriggerEx turnL { get; protected set; }
    public TriggerEx turnR { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        attack = new TriggerEx(anim, "Attack");
        fire = new TriggerEx(anim, "Fire");
        turnL = new TriggerEx(anim, "TurnL");
        turnR = new TriggerEx(anim, "TurnR");
    }
}
