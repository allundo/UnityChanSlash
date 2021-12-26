using UnityEngine;

public class PlayerEffect : ShieldEffect
{
    [SerializeField] private AudioSource jumpSound = null;
    [SerializeField] private AudioSource jumpLandingSound = null;

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
    public void OnJump()
    {
        jumpSound.PlayEx();
    }

    public void OnJumpLanding()
    {
        jumpLandingSound.PlayEx();
    }
}
