using UnityEngine;
public class ShieldEffect : MobEffect
{
    [SerializeField] private AudioSource shieldSound = null;
    [SerializeField] protected ParticleSystem shieldVfx = default;

    // Called as Animation Event functions
    public virtual void OnShield()
    {
        shieldSound.PlayEx();
        shieldVfx?.Play();
    }
}