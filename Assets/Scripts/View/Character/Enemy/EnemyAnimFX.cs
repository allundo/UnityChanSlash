using UnityEngine;

public class EnemyAnimFX : AnimationFX
{
    [SerializeField] private AudioSource attackSfx = null;
    [SerializeField] protected ParticleSystem attackVfx = default;

    public override void StopVFX() => OnAttackEnd();

    // Called as Animation Event functions
    public virtual void OnAttack() => fx.Play(attackSfx, attackVfx);
    public virtual void OnAttackEnd() => attackVfx?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
}
