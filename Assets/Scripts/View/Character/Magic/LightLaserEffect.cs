using DG.Tweening;

public class LightLaserEffect : MagicEffect
{
    private Tween deadTimer;
    public override void Disappear(TweenCallback onComplete = null, float duration = 0.5f)
    {
        emitVfx.StopEmitting();
        fireVfx.StopEmitting();
        fireSound.StopEx();
        deadTimer = DOVirtual.DelayedCall(duration, onComplete, false).Play();
    }

    public override void OnActive()
    {
        emitVfx.PlayEx();
        fireVfx.PlayEx();
        fireSound.PlayEx();
    }

    public override void OnDie() { }

    public override void OnDamage(float damageRatio, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None) { }

    public override void OnDestroyByReactor() => deadTimer?.Kill();
}
