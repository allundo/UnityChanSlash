using UnityEngine;


public interface IGhostReactor : IMobReactor
{
    void OnAttackStart();
    void OnAttackEnd();
}

[RequireComponent(typeof(GhostEffect))]
[RequireComponent(typeof(GhostStatus))]
public class GhostReactor : EnemyReactor, IGhostReactor
{
    protected GhostEffect ghostEffect;

    protected override void Awake()
    {
        base.Awake();
        ghostEffect = effect as GhostEffect;

    }

    public override void OnHide()
    {
        if (status.isHidden) return;

        map.RemoveObjectOn();
        status.SetHidden(true);
        mobEffect.OnHide();
    }
    public void OnAttackStart()
    {
        OnHide();
        ghostEffect.OnAttackStart();
    }

    public void OnAttackEnd() => ghostEffect.OnAttackEnd();

    public override bool OnAppear()
    {
        if (!status.isHidden) return true;
        if (map.OnTile.OnCharacterDest != null) return false;

        map.RemoveObjectOn();
        status.SetHidden(false);
        map.SetObjectOn(map.onTilePos);
        ghostEffect.OnAppear();
        return true;
    }
}
