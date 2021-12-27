public class PlayerEffect : MobEffect
{
    protected PlayerAnimator anim;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<PlayerAnimator>();
    }

    public override void OnDamage(float damageRatio, AttackType type = AttackType.None)
    {
        base.OnDamage(damageRatio, type);
        anim.rest.Bool = false;
    }
}
