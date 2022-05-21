using UnityEngine;

public class WitchAnimFX : AnimationFX
{
    [SerializeField] private AudioSource targetAttackSfx = null;
    [SerializeField] protected ParticleSystem targetShockVfx = default;
    [SerializeField] protected ParticleSystem targetTrailVfx = default;
    [SerializeField] protected ParticleSystem magicTrailVfx = default;

    // Called as Animation Event functions
    public void OnAttack() => fx.Play(targetAttackSfx, targetShockVfx);
    public void OnTrailStart() => targetTrailVfx?.Play();
    public void OnTrailEnd() => fx.StopEmitting(targetTrailVfx);

    public void OnMagicStart() => magicTrailVfx?.Play();
    public void OnMagicEnd() => fx.StopEmitting(magicTrailVfx);

    public override void StopVFX()
    {
        OnTrailEnd();
        OnMagicEnd();
        fx.StopEmitting(targetShockVfx);
    }
}
