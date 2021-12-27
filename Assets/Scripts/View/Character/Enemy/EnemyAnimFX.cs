using UnityEngine;

public class EnemyAnimFX : AnimationFX
{
    [SerializeField] private AudioSource attackSfx = null;
    [SerializeField] protected ParticleSystem attackVfx = default;

    // Called as Animation Event functions
    public virtual void OnAttack() => Play(attackSfx, attackVfx);
}