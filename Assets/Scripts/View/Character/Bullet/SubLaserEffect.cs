using DG.Tweening;

// Dummy, no effect set
public class SubLaserEffect : LightLaserEffect
{
    public override void Disappear(TweenCallback onComplete = null, float duration = 0.5f)
    {
        if (onComplete != null) onComplete();
    }

    public override void OnActive() { }

    public override void OnDie() { }

    public override void OnDamage(float damageRatio, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None) { }

    public override void OnDestroyByReactor() { }
}
