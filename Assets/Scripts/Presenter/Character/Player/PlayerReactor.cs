using UnityEngine;

[RequireComponent(typeof(GuardState))]
public class PlayerReactor : MobReactor
{
    protected GuardState guardState;

    protected override void Awake()
    {
        base.Awake();
        guardState = GetComponent<GuardState>();
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
    protected override void Dead() => Inactivate();
}
