using UnityEngine;

[RequireComponent(typeof(GhostEffect))]
[RequireComponent(typeof(GhostStatus))]
public class GhostReactor : EnemyReactor
{
    protected GhostEffect ghostEffect;
    protected GhostStatus ghostStatus;

    protected override void Awake()
    {
        base.Awake();
        ghostEffect = effect as GhostEffect;
        ghostStatus = status as GhostStatus;

    }
    public override float OnDamage(float attack, IDirection dir, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        if (!status.isOnGround) return 0f;
        return base.OnDamage(attack, dir, type);
    }

    public void OnHide()
    {
        if (!status.isOnGround) return;

        map.RemoveObjectOn();
        ghostStatus.SetOnGround(false);
        ghostEffect.OnHide();
    }
    public void OnAttackStart()
    {
        OnHide();
        ghostEffect.OnAttackStart();
    }

    public void OnAttackEnd() => ghostEffect.OnAttackEnd();

    public void OnAppear()
    {
        if (status.isOnGround) return;

        map.RemoveObjectOn();
        ghostStatus.SetOnGround(true);
        map.SetObjectOn(map.onTilePos);
        ghostEffect.OnAppear();
    }
}
