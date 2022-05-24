using UnityEngine;

public class RabbitAnimFX : AnimationFX
{
    [SerializeField] private AudioSource jumpAttackSfx = null;
    [SerializeField] private ParticleSystem jumpAttackVfx = default;
    [SerializeField] private AudioSource somersaultSfx = null;
    [SerializeField] private ParticleSystem somersaultVfx = default;

    // Called as Animation Event functions
    public virtual void OnJumpAttack(AnimationEvent evt)
    {
        // Don't play effect on mixed animation clip with dying
        if (evt.animatorClipInfo.weight > 0.75f) fx.Play(jumpAttackSfx, jumpAttackVfx);
    }
    public virtual void OnSomersault(AnimationEvent evt)
    {
        // Don't play effect on mixed animation clip with dying
        if (evt.animatorClipInfo.weight > 0.75f) fx.Play(somersaultSfx, somersaultVfx);
    }

    public virtual void OnSomersaultEnd() => somersaultVfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);

    public override void StopVFX()
    {
        jumpAttackVfx?.Stop();
        somersaultVfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
