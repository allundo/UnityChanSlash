using UnityEngine;

public class ShieldAnimFX : AnimationFX
{
    [SerializeField] protected AudioSource shieldSfx = null;
    [SerializeField] protected ParticleSystem shieldVfx = default;

    // Called as Animation Event functions by shield enemies
    // Or called via PlayerEffect by player
    public virtual void OnShield() => fx.Play(shieldSfx, shieldVfx);

    public override void StopVFX()
    {
        shieldVfx.StopEmitting();
    }
}
