using UnityEngine;


public interface IGhostReactor : IReactor
{
    void OnHide();
    void OnAppear();
    void OnAttackStart();
    void OnAttackEnd();
}

[RequireComponent(typeof(GhostEffect))]
[RequireComponent(typeof(GhostStatus))]
public class GhostReactor : EnemyReactor, IGhostReactor
{
    protected GhostEffect ghostEffect;
    protected IGhostStatus ghostStatus;

    protected override void Awake()
    {
        base.Awake();
        ghostEffect = effect as GhostEffect;
        ghostStatus = status as IGhostStatus;

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
        if (status.isOnGround || map.OnTile.OnCharacterDest != null) return;

        map.RemoveObjectOn();
        ghostStatus.SetOnGround(true);
        map.SetObjectOn(map.onTilePos);
        ghostEffect.OnAppear();
    }
}
