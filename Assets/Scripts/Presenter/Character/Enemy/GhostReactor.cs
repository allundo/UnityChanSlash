using UnityEngine;


public interface IGhostReactor : IReactor
{
    bool OnHide();
    bool OnAppear();
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

    public bool OnHide()
    {
        if (!status.isOnGround) return true;

        map.RemoveObjectOn();
        ghostStatus.SetOnGround(false);
        ghostEffect.OnHide();
        return true;
    }
    public void OnAttackStart()
    {
        OnHide();
        ghostEffect.OnAttackStart();
    }

    public void OnAttackEnd() => ghostEffect.OnAttackEnd();

    public bool OnAppear()
    {
        if (status.isOnGround) return true;
        if (map.OnTile.OnCharacterDest != null) return false;

        map.RemoveObjectOn();
        ghostStatus.SetOnGround(true);
        map.SetObjectOn(map.onTilePos);
        ghostEffect.OnAppear();
        return true;
    }
}
