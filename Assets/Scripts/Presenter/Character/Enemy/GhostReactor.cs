using UnityEngine;

[RequireComponent(typeof(GhostEffect))]
[RequireComponent(typeof(GhostStatus))]
public class GhostReactor : EnemyReactor
{
    public override float OnDamage(float attack, IDirection dir, AttackType type = AttackType.None)
    {
        if (!status.isOnGround) return 0f;
        return base.OnDamage(attack, dir, type);
    }

    public void OnHide()
    {
        if (!status.isOnGround) return;

        map.RemoveObjectOn();
        (status as GhostStatus).SetOnGround(false);
        (effect as GhostEffect).OnHide();
    }

    public void OnAppear()
    {
        if (status.isOnGround) return;

        map.RemoveObjectOn();
        (status as GhostStatus).SetOnGround(true);
        map.SetObjectOn(map.onTilePos);
        (effect as GhostEffect).OnAppear();
    }
}
