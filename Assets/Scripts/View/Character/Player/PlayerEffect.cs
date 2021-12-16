using UnityEngine;

public class PlayerEffect : MobEffect
{
    [SerializeField] private AudioSource shieldSound = null;
    [SerializeField] private AudioSource jumpSound = null;
    [SerializeField] private AudioSource jumpLandingSound = null;
    [SerializeField] protected ParticleSystem shieldVfx = default;

    protected PlayerAnimator anim;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<PlayerAnimator>();
    }

    public override void OnDamage(float damageRatio, AttackType type = AttackType.None)
    {
        base.OnDamage(damageRatio, type);
        anim.rest.Bool = false;
    }

    // Called as Animation Event functions
    public void OnShield()
    {
        shieldSound.PlayEx();
        shieldVfx?.Play();
    }
    public void OnJump()
    {
        jumpSound.PlayEx();
    }

    public void OnJumpLanding()
    {
        jumpLandingSound.PlayEx();
    }
}
