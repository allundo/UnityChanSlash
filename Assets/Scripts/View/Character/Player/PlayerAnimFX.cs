using UnityEngine;

public class PlayerAnimFX : AnimationFX
{
    [SerializeField] private AudioSource shieldSfx = null;
    [SerializeField] protected ParticleSystem shieldVfx = default;

    [SerializeField] private AudioSource jabSfx = null;
    [SerializeField] protected ParticleSystem jabVfx = default;

    [SerializeField] private AudioSource straightSfx = null;
    [SerializeField] protected ParticleSystem straightVfx = default;

    [SerializeField] private AudioSource kickSfx = null;
    [SerializeField] protected ParticleSystem kickVfx = default;

    [SerializeField] private AudioSource criticalSfx = null;
    [SerializeField] protected ParticleSystem jabCriticalVfx = default;
    [SerializeField] protected ParticleSystem straightCriticalVfx = default;
    [SerializeField] protected ParticleSystem kickCriticalVfx = default;

    [SerializeField] private AudioSource jumpSfx = null;
    [SerializeField] private AudioSource jumpLandingSfx = null;

    [SerializeField] private AudioSource brakeSfx = null;
    [SerializeField] private AudioSource brakeAndStepSfx = null;

    [SerializeField] private AudioSource stepSfx = null;

    // Called as Animation Event functions
    public void OnShield() => fx.Play(shieldSfx, shieldVfx);

    public void OnJab() => fx.PlayPitch(jabSfx, jabVfx);
    public void OnStraight() => fx.PlayPitch(straightSfx, straightVfx);
    public void OnKick() => fx.PlayPitch(kickSfx, kickVfx);

    public void OnJabCritical() => fx.PlayPitch(criticalSfx, jabCriticalVfx, 1f, 1.05f);
    public void OnStraightCritical() => fx.PlayPitch(criticalSfx, straightCriticalVfx, 0.95f, 1f);
    public void OnKickCritical() => fx.PlayPitch(criticalSfx, kickCriticalVfx, 0.9f, 0.95f);

    public void OnJump() => fx.Play(jumpSfx);
    public void OnJumpLanding() => fx.Play(jumpLandingSfx);

    public void OnBrake() => fx.PlayPitch(brakeSfx, 0.9f, 1.1f);
    public void OnBrakeAndStep() => fx.Play(brakeAndStepSfx);

    public void OnStep(AnimationEvent evt)
    {
        if (evt.animatorClipInfo.weight > 0.5f) fx.PlayPitch(stepSfx, 0.8f, 1.2f);
    }

    public void OnRunningStep(AnimationEvent evt)
    {
        if (evt.animatorClipInfo.weight > 0.5f) fx.PlayPitch(stepSfx, 0.9f, 1.3f);
    }

    public override void StopVFX()
    {
        jabVfx?.Stop();
        straightVfx?.Stop();
        kickVfx?.Stop();
        shieldVfx?.Stop();
    }
}
