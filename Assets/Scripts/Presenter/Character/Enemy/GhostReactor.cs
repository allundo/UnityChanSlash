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

    public override void Hide()
    {
        if (mobStatus.isHidden) return;

        mobMap.RemoveObjectOn();
        mobStatus.SetHidden(true);
        mobEffect.OnHide();
    }
    public void OnAttackStart()
    {
        Hide();
        ghostEffect.OnAttackStart();
    }

    public void OnAttackEnd() => ghostEffect.OnAttackEnd();

    public override bool Appear()
    {
        if (!mobStatus.isHidden) return true;
        if (mobMap.OnTile.OnCharacterDest != null) return false;

        mobMap.RemoveObjectOn();
        mobStatus.SetHidden(false);
        mobMap.SetObjectOn(map.onTilePos);
        ghostEffect.OnAppear();
        return true;
    }
}
