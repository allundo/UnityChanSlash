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

    public override void OnDamage(float attack, Direction attackDir)
    {
        if (!status.IsAlive) return;

        float shield = 0.0f;

        if (guardState.IsShieldOn(attackDir))
        {
            shield = status.Shield;
            guardState.SetShield();
        }

        float damage = Mathf.Max(status.CalcAttack(attack, attackDir) - shield, 0.0f);

        visual.DamageFlash(damage, status.LifeMax);

        status.Damage(damage);
    }
}