public class RabbitAnimator : EnemyAnimator
{
    public AnimatorTrigger turnR { get; protected set; }
    public AnimatorTrigger turnL { get; protected set; }
    public AnimatorBool wondering { get; protected set; }
    public AnimatorTrigger somersault { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        turnR = new AnimatorTrigger(anim, "TurnR");
        turnL = new AnimatorTrigger(anim, "TurnL");
        wondering = new AnimatorBool(anim, "Wondering");
        somersault = new AnimatorTrigger(anim, "Somersault");
    }
}
