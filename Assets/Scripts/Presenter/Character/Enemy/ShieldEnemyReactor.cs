using UnityEngine;
using static ShieldInput;

[RequireComponent(typeof(ShieldEffect))]
public class ShieldEnemyReactor : EnemyReactor
{
    protected GuardState guardState => (input as ShieldInput).guardState;

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
}
