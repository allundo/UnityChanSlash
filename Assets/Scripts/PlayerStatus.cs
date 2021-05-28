
using UnityEngine;

[RequireComponent(typeof(PlayerCommander))]
[RequireComponent(typeof(UnityChanAnimeHandler))]
public class PlayerStatus : MobStatus
{

    protected PlayerCommander playerCommander = default;
    protected UnityChanAnimeHandler animeHandler = default;

    protected override float Shield(Direction attackDir) => playerCommander.IsShieldOn(attackDir) ? 1 : 0;

    protected override void Start()
    {
        base.Start();
        animeHandler = GetComponent<UnityChanAnimeHandler>();
        playerCommander = GetComponent<PlayerCommander>();
    }

    protected override void OnDamage(float damage, float shield)
    {
        if (shield > 0)
        {
            animeHandler.shield.Fire();
        }

        base.OnDamage(damage, shield);
    }
}
