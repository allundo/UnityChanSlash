public class EnemyTurnAnimator : EnemyAnimator
{
    public AnimatorTrigger turnR { get; protected set; }
    public AnimatorTrigger turnL { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        turnR = new AnimatorTrigger(anim, "TurnR");
        turnL = new AnimatorTrigger(anim, "TurnL");
    }
}
