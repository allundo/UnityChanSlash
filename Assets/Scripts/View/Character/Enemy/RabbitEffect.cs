public class RabbitEffect : EnemyEffect
{
    protected RabbitAnimator anim;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<RabbitAnimator>();
    }

    public override void OnDamage(float damageRatio, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        base.OnDamage(damageRatio, type, attr);
        anim.wondering.Bool = false;
    }
}
