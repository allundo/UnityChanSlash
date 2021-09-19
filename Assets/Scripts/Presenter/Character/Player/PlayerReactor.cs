using UnityEngine;

[RequireComponent(typeof(GuardState))]
[RequireComponent(typeof(PlayerCommander))]
public class PlayerReactor : MobReactor
{
    protected GuardState guardState;
    protected PlayerCommander playerCommander;

    protected override void Awake()
    {
        base.Awake();
        guardState = GetComponent<GuardState>();
        playerCommander = GetComponent<PlayerCommander>();
    }

    protected override float CalcDamage(float attack, IDirection dir)
    {
        float shield = 0.0f;

        if (guardState.IsShieldOn(dir))
        {
            shield = status.Shield;
            guardState.SetShield();
        }

        return Mathf.Max(status.CalcAttack(attack, dir) - shield, 0.0f);
    }

    protected override void OnDie()
    {
        base.OnDie();
        playerCommander.InvisibleInput();
    }

    protected override void Dead() => Inactivate();
}
