using UnityEngine;

public class WitchDoubleEffect : BulletEffect
{
    public void OnAttackStart()
    {
        emitVfx?.Play();
    }

    public void OnAttackEnd()
    {
        emitVfx?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
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
