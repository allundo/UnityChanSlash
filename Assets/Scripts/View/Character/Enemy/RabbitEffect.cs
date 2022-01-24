public class RabbitEffect : MobEffect
{
    protected RabbitAnimator anim;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<RabbitAnimator>();
    }

    public override void OnDamage(float damageRatio, AttackType type = AttackType.None)
    {
        base.OnDamage(damageRatio, type);
        anim.wondering.Bool = false;
    }
}
