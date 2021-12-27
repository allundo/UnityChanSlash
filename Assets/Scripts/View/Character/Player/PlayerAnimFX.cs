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
    public void OnShield() => Play(shieldSfx, shieldVfx);
    public void OnJab() => Play(jabSfx, jabVfx);
    public void OnStraight() => Play(straightSfx, straightVfx);
    public void OnKick() => Play(kickSfx, kickVfx);
    public void OnJump() => Play(jumpSfx);
    public void OnJumpLanding() => Play(jumpLandingSfx);
}