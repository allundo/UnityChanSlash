using UnityEngine;

public class PlayerEffect : MobEffect
{
    [SerializeField] private AudioSource shieldSound = null;
    [SerializeField] private AudioSource jumpSound = null;
    [SerializeField] private AudioSource jumpLandingSound = null;
    [SerializeField] protected ParticleSystem shieldVfx = default;

    public void OnShield()
    {
        Play(shieldSound);
        shieldVfx?.Play();
    }
    public void OnJump()
    {
        Play(jumpSound);
    }

    public void OnJumpLanding()
    {
        Play(jumpLandingSound);
    }
}