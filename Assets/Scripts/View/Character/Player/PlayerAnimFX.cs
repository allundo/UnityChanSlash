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

    [SerializeField] private AudioSource jumpSfx = null;
    [SerializeField] private AudioSource jumpLandingSfx = null;

    // Called as Animation Event functions
    public void OnShield() => fx.Play(shieldSfx, shieldVfx);
    public void OnJab() => fx.PlayPitch(jabSfx, jabVfx);
    public void OnStraight() => fx.PlayPitch(straightSfx, straightVfx);
    public void OnKick() => fx.PlayPitch(kickSfx, kickVfx);
    public void OnJump() => fx.Play(jumpSfx);
    public void OnJumpLanding() => fx.Play(jumpLandingSfx);

    public override void StopVFX()
    {
        jabVfx?.Stop();
        straightVfx?.Stop();
        kickVfx?.Stop();
        shieldVfx?.Stop();
    }
}
