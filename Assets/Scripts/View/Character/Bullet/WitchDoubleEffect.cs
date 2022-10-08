using UnityEngine;

public class WitchDoubleEffect : BulletEffect
{
    protected override void Awake()
    {
        bulletMatEffect = new MatColorEffect(meshTf);
    }

    public void OnAttackStart()
    {
        emitVfx.PlayEx();
    }

    public void OnAttackEnd()
    {
        emitVfx.StopEmitting();
    }

    public override void OnActive()
    {
        bulletMatEffect.Activate(0.01f);
    }

    public override void OnDie()
    {
        bulletMatEffect.Inactivate();
    }

    public override void OnDamage(float damageRatio, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None) { }
}
