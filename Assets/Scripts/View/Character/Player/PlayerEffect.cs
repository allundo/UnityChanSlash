using UnityEngine;

public class PlayerEffect : MobEffect
{
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
        (anim as PlayerAnimator).rest.Bool = false;
    }

    public override void OnActive()
    {
        matColEffect.Activate(0f);
    }

    public void OnIced(Vector3 pos, bool isPaused)
    {
        if (isPaused) anim.Pause();
        resourceFX.PlayVFX(VFXType.Iced, pos);
        matColEffect.Flash(new Color(0f, 0.5f, 0.5f, 1f), 0.1f);
    }

    public void OnShield() => (animFX as ShieldAnimFX).OnShield();

    public void OnDrop()
    {
        resourceFX.PlaySnd(SNDType.DropToGround);
    }

    public void OnGround()
    {
        resourceFX.StopSnd(SNDType.DropToGround);
    }
}
