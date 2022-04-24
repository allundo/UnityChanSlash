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

        map.RemoveObjectOn();
        mobStatus.SetHidden(true);
        effect.OnHide();
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
        if (map.OnTile.OnCharacterDest != null) return false;

        map.RemoveObjectOn();
        mobStatus.SetHidden(false);
        map.SetObjectOn(map.onTilePos);
        ghostEffect.OnAppear();
        return true;
    }
}
