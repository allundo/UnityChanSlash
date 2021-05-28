
using UnityEngine;

[RequireComponent(typeof(PlayerCommander))]
[RequireComponent(typeof(UnityChanAnimeHandler))]
public class PlayerStatus : MobStatus
{

    protected PlayerCommander playerCommander = default;
    protected UnityChanAnimeHandler playerAnim = default;

    protected override float Shield(Direction attackDir) => playerCommander.IsShieldOn(attackDir) ? 1 : 0;

    protected override void Start()
    {
        base.Start();

        playerCommander = commander as PlayerCommander;
        playerAnim = playerCommander.anim as UnityChanAnimeHandler;
    }

    protected override void OnDamage(float damage, float shield)
    {
        if (shield > 0)
        {
            playerAnim.shield.Fire();
        }

        base.OnDamage(damage, shield);
    }
}
