using UnityEngine;
using static ShieldInput;

[RequireComponent(typeof(ShieldEnemyAnimator))]
[RequireComponent(typeof(ShieldEnemyAnimFX))]
public class ShieldEnemyReactor : EnemyReactor
{
    [SerializeField] protected Transform shieldTf;
    protected ShieldEnemyAnimator anim;
    protected GuardState guardState => (input as ShieldInput).guardState;
    protected MatColorEffect shieldEffect;

    protected override void Awake()
    {
        anim = GetComponent<ShieldEnemyAnimator>();
        shieldEffect = new MatColorEffect(shieldTf);
        base.Awake();
    }

    protected override float CalcDamage(float attack, IDirection dir, AttackAttr attr = AttackAttr.None)
    {
        float shield = 0.0f;

        if (guardState.IsShieldOn(dir))
        {
            shield = status.Shield;
            guardState.SetShield();
            shieldEffect.DamageFlash(0.1f);
        }

        return Mathf.Max(status.CalcAttack(attack, dir, attr) - shield, 0.0f);
    }
    public override void Destroy()
    {
        anim.ClearTriggers();
        base.Destroy();
    }
}
