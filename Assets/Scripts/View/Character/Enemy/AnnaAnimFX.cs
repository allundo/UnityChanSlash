using UnityEngine;

public class AnnaAnimFX : ShieldAnimFX
{
    [SerializeField] protected ParticleSystem auraVfx = default;

    public override void StopVFX()
    {
        auraVfx.StopEmitting();
        shieldVfx.StopEmitting();
    }
}
