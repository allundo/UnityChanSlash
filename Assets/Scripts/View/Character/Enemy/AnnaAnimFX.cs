using UnityEngine;

public class AnnaAnimFX : ShieldAnimFX
{
    [SerializeField] protected ParticleSystem auraVfx = default;
    [SerializeField] protected ParticleSystem fireAuraVfx = default;
    [SerializeField] protected ParticleSystem slashEnchantVfx = default;
    [SerializeField] protected AudioSource slashEnchantSnd = default;
    [SerializeField] protected ParticleSystem slashTornadoVfx = default;
    [SerializeField] protected AudioSource slashTornadoSnd = default;
    [SerializeField] protected ParticleSystem slashEndVfx = default;

    public void OnFire()
    {
        fireAuraVfx.PlayEx();
    }

    public void OnCrouching()
    {
        slashEnchantVfx.PlayEx();
        slashEnchantSnd.PlayEx();
    }

    public void OnSlash()
    {
        slashTornadoVfx.PlayEx();
        slashTornadoSnd.PlayEx();
    }

    public void OnSlashEnd()
    {
        slashEndVfx.PlayEx();
    }

    public override void StopVFX()
    {
        slashEnchantVfx.StopEmitting();
        slashTornadoVfx.StopEmitting();
        auraVfx.StopEmitting();
        shieldVfx.StopEmitting();
    }
}
