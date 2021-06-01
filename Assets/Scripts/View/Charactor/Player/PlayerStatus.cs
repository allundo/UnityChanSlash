
using UnityEngine;

[RequireComponent(typeof(PlayerCommander))]
[RequireComponent(typeof(PlayerAnimator))]
public class PlayerStatus : MobStatus
{

    protected GuardState guardState = default;

    protected override float Shield(Direction attackDir) => guardState.IsShieldOn(attackDir) ? 1 : 0;

    protected override void Start()
    {
        base.Start();

        guardState = GetComponent<GuardState>();
    }

    protected override void OnDamage(float damage, float shield)
    {
        if (shield > 0)
        {
            guardState.SetShield();
        }

        base.OnDamage(damage, shield);
    }
}
