public class RabbitEffect : EnemyEffect
{
    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<RabbitAnimator>();
    }

    public override void OnDamage(float damageRatio, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None, IDirection dir = null)
    {
        base.OnDamage(damageRatio, type, attr, dir);
        (anim as RabbitAnimator).wondering.Bool = false;
    }
}
