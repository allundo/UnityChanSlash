using UnityEngine;

public class AnnaAnimFX : ShieldAnimFX
{
    [SerializeField] protected ParticleSystem auraVfx = default;
    [SerializeField] protected ParticleSystem fireAuraVfx = default;

    public void OnFire()
    {
        fireAuraVfx.PlayEx();
    }

    public override void StopVFX()
    {
        auraVfx.StopEmitting();
        shieldVfx.StopEmitting();
    }
}
