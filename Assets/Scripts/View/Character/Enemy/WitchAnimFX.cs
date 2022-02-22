using UnityEngine;

public class WitchAnimFX : AnimationFX
{
    [SerializeField] private AudioSource targetAttackSfx = null;
    [SerializeField] protected ParticleSystem targetShockVfx = default;
    [SerializeField] protected ParticleSystem targetTrailVfx = default;

    // Called as Animation Event functions
    public void OnAttack() => Play(targetAttackSfx, targetShockVfx);
    public void OnTrailStart() => targetTrailVfx?.Play();
    public void OnTrailEnd() => targetTrailVfx?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
}
