public class RabbitAnimator : EnemyTurnAnimator
{
    public AnimatorBool wondering { get; protected set; }
    public AnimatorTrigger somersault { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        wondering = new AnimatorBool(anim, "Wondering");
        somersault = new AnimatorTrigger(anim, "Somersault");
    }
}
