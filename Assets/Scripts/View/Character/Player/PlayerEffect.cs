using UnityEngine;

public class PlayerEffect : MobEffect
{
    protected PlayerAnimator anim;

    protected override void Awake()
    {
        matColEffect = new MobMatColorEffect(transform);
        sndData = Resources.Load<PlayerDamageSndData>("DataAssets/Sound/PlayerDamageSndData");
        animFX = GetComponent<AnimationFX>();
        resourceFX = new ResourceFX(transform);
        anim = GetComponent<PlayerAnimator>();
    }

    public override void OnDamage(float damageRatio, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        base.OnDamage(damageRatio, type, attr);
        anim.rest.Bool = false;
    }

    public override void OnActive()
    {
        matColEffect.Activate(0f);
    }
}
