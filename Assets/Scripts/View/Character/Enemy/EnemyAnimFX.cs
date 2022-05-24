using UnityEngine;

public class EnemyAnimFX : AnimationFX
{
    [SerializeField] private AudioSource attackSfx = null;
    [SerializeField] protected ParticleSystem attackVfx = default;

    public override void StopVFX() => attackVfx?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

    // Called as Animation Event functions
    public virtual void OnAttack(AnimationEvent evt)
    {
        // Don't play effect on mixed animation clip with dying
        if (evt.animatorClipInfo.weight > 0.75f) fx.Play(attackSfx, attackVfx);
    }
    public virtual void OnAttackEnd() => attackVfx?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
}
