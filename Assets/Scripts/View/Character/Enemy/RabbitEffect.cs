public class RabbitEffect : EnemyEffect
{
    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<RabbitAnimator>();
    }

    public override void OnDamage(float damageRatio, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        base.OnDamage(damageRatio, type, attr);
        (anim as RabbitAnimator).wondering.Bool = false;
    }
}
