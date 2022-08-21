using UnityEngine;
using static ShieldInput;

[RequireComponent(typeof(ShieldEnemyAnimator))]
[RequireComponent(typeof(ShieldEnemyAnimFX))]
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
        float shield = 0.0f;

        if (guardState.IsShieldOn(dir))
        {
            var shieldEffectiveness = guardState.SetShield();
            shield = mobStatus.Shield * shieldEffectiveness;
            shieldEffect.DamageFlash(0.1f);
        }

        return Mathf.Max(mobStatus.CalcAttack(attack, dir, attr) - shield, 0.0f);
    }
    public override void Destroy()
    {
        shieldAnim.ClearTriggers();
        base.Destroy();
    }
}
