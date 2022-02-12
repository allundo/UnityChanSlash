public class RabbitAnimator : EnemyTurnAnimator
{
    public AnimatorBool icedFall { get; protected set; }
    public AnimatorBool wondering { get; protected set; }
    public AnimatorTrigger somersault { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        icedFall = new AnimatorBool(anim, "IcedFall");
        wondering = new AnimatorBool(anim, "Wondering");
        somersault = new AnimatorTrigger(anim, "Somersault");
    }
}
