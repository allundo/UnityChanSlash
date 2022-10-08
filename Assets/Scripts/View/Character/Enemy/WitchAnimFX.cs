using UnityEngine;

public class WitchAnimFX : AnimationFX
{
    [SerializeField] protected ParticleSystem magicTrailVfx = default;

    // Called as Animation Event functions
    public void OnMagicStart() => magicTrailVfx.PlayEx();
    public void OnMagicEnd() => magicTrailVfx.StopEmitting();

    public override void StopVFX()
    {
        OnMagicEnd();
    }
}
