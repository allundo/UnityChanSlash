using UnityEngine;

public class WitchAnimFX : AnimationFX
{
    [SerializeField] private AudioSource targetAttackSfx = null;
    [SerializeField] protected ParticleSystem targetShockVfx = default;
    [SerializeField] protected ParticleSystem targetTrailVfx = default;

    // Called as Animation Event functions
    public void OnAttack() => fx.Play(targetAttackSfx, targetShockVfx);
    public void OnTrailStart() => targetTrailVfx?.Play();
    public void OnTrailEnd() => fx.StopEmitting(targetTrailVfx);

    public override void StopVFX()
    {
        OnTrailEnd();
        fx.StopEmitting(targetShockVfx);
    }
}
