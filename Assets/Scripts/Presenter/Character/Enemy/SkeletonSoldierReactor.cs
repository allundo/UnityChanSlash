using UnityEngine;

[RequireComponent(typeof(UndeadStatus))]
[RequireComponent(typeof(SkeletonSoldierAnimator))]
[RequireComponent(typeof(SkeletonSoldierAnimFX))]
public class SkeletonSoldierReactor : ShieldEnemyReactor, IUndeadReactor
{
    protected UndeadStatus undeadStatus;
    protected IUndeadInput undeadInput;

    protected override void Awake()
    {
        base.Awake();
        undeadStatus = status as UndeadStatus;
        undeadInput = input as IUndeadInput;
    }
    protected override void OnLifeChange(float life)
    {
        if (life <= 0.0f)
        {
            if (undeadStatus.curse > 0f)
            {
                undeadInput.InputSleep();
            }
            else
            {
                input.InputDie();
            }
        }

        lifeGauge?.OnLifeChange(life, status.LifeMax.Value);
    }
    public override float OnDamage(float attack, IDirection dir, AttackType type = AttackType.None)
    {
        float damage = base.OnDamage(attack, dir, type);
        undeadStatus.CurseDamage(damage);
        return damage;
    }

    public void OnResurrection()
    {
        status.ResetStatus();
    }
}
