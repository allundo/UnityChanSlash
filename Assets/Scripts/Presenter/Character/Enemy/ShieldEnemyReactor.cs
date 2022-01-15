using UnityEngine;
using static ShieldInput;

[RequireComponent(typeof(ShieldEnemyAnimator))]
[RequireComponent(typeof(ShieldEnemyAnimFX))]
public class ShieldEnemyReactor : EnemyReactor
{
    protected ShieldEnemyAnimator anim;
    protected GuardState guardState => (input as ShieldInput).guardState;

    protected override void Awake()
    {
        anim = GetComponent<ShieldEnemyAnimator>();
        base.Awake();
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
    public override void Destroy()
    {
        anim.ClearTriggers();
        base.Destroy();
    }
}
