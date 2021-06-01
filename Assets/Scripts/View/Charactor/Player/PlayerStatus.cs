using UnityEngine;

[RequireComponent(typeof(GuardState))]
[RequireComponent(typeof(HidePool))]
public class PlayerStatus : MobStatus
{

    protected GuardState guardState;
    protected HidePool hidePool;

    protected override float Shield(Direction attackDir) => guardState.IsShieldOn(attackDir) ? 1 : 0;

    protected override void Awake()
    {
        base.Awake();

        guardState = GetComponent<GuardState>();
        hidePool = GetComponent<HidePool>();
    }

    protected override void OnDamage(float damage, float shield)
    {
        if (shield > 0)
        {
            guardState.SetShield();
        }

        base.OnDamage(damage, shield);
    }

    public override void Activate()
    {
        ResetStatus();
        map.SetPosition(GameManager.Instance.GetPlayerInitPos, new South());
        hidePool.Init();
    }

}
