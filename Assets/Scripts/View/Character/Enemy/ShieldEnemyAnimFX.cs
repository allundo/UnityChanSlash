using UnityEngine;

public class ShieldEnemyAnimFX : EnemyAnimFX
{
    [SerializeField] private AudioSource shieldSfx = null;
    [SerializeField] protected ParticleSystem shieldVfx = default;


    // Called as Animation Event functions
    public virtual void OnShield() => fx.Play(shieldSfx, shieldVfx);
}
