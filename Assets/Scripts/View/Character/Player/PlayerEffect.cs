public class PlayerEffect : MobEffect
{
    protected PlayerAnimator anim;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<PlayerAnimator>();
    }

    public override void OnDamage(float damageRatio, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        base.OnDamage(damageRatio, type, attr);
        anim.rest.Bool = false;
    }
}
