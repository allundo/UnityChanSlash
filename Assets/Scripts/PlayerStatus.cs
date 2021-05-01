
using UnityEngine;

[RequireComponent(typeof(UnityChanAnimeHandler))]
public class PlayerStatus : MobStatus
{
    protected AnimeHandler animeHandler = default;
    protected bool IsShieldReady => animeHandler.currentState.IsShieldReady;

    protected override float Shield(Direction attackDir) => IsShieldOn(attackDir) ? 1 : 0;
    protected override bool IsShieldOn(Direction attackDir) => IsShieldReady && attackDir.IsInverse(dir);

    protected override void Start()
    {
        base.Start();
        animeHandler = GetComponent<AnimeHandler>();
    }

    protected override void OnDamage(float damage, float shield)
    {
        if (shield > 0)
        {
            anim.SetTrigger("Shield");
        }

        base.OnDamage(damage, shield);
    }
}
