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
    protected override void OnActive(EnemyStatus.ActivateOption option)
    {
        base.OnActive(option);
        if (option.isHidden) Hide();
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

        mobStatus.SetHidden(false);
        ghostEffect.OnAppear();
        return true;
    }
}
