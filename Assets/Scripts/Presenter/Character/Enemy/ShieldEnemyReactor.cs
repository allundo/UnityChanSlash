using UnityEngine;
using static ShieldInput;

[RequireComponent(typeof(ShieldEnemyAnimator))]
[RequireComponent(typeof(ShieldAnimFX))]
public class ShieldEnemyReactor : EnemyReactor
{
    [SerializeField] protected Transform shieldTf;
    protected ShieldAnimator shieldAnim;
    protected GuardState guardState => (input as ShieldInput).guardState;
    protected MatColorEffect shieldEffect;

    protected override void Awake()
    {
        base.Awake();
        shieldAnim = anim as ShieldAnimator;
        shieldEffect = new MatColorEffect(shieldTf);
    }

    protected override float CalcDamage(float attack, IDirection dir, AttackAttr attr = AttackAttr.None)
    {
        if (guardState.IsShieldOn(dir))
        {
            shieldEffect.DamageFlash();
            return mobStatus.CalcAttackWithShield(attack, guardState.SetShield(), attr);
        }

        return mobStatus.CalcAttack(attack, dir, attr);
    }

    public override void Destroy()
    {
        shieldAnim.ClearTriggers();
        base.Destroy();
    }
}
