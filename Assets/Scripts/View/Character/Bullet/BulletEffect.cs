using UnityEngine;
using DG.Tweening;

public class BulletEffect : MonoBehaviour, IBodyEffect
{
    [SerializeField] protected ParticleSystem emitVfx = default;
    [SerializeField] protected ParticleSystem fireVfx = default;
    [SerializeField] protected ParticleSystem hitVfx = default;
    [SerializeField] protected ParticleSystem eraseVfx = default;

    [SerializeField] protected AudioSource fireSound = default;

    [SerializeField] protected Transform meshTf = null;
    [SerializeField] protected float dyingFXDuration = 0f;
    [SerializeField] protected float cycle = 0f;
    [SerializeField] protected Color blinkColor = default;

    protected BulletMatEffect bulletMatEffect = null;

    protected virtual void Awake()
    {
        bulletMatEffect = new BulletMatEffect(meshTf, dyingFXDuration, cycle, blinkColor);
    }

    protected virtual Tween RollingTween(float duration = 0.25f)
    {
        return meshTf.DOLocalRotate(new Vector3(0f, 0f, 90f), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetRelative()
            .SetLoops(-1, LoopType.Incremental);
    }

    public void Disappear(TweenCallback onComplete = null, float duration = 0.5f)
         => bulletMatEffect.Inactivate(onComplete, duration);

    public virtual void OnActive()
    {
        fireSound.PlayEx();
        emitVfx?.Play();
        fireVfx?.Play();
        bulletMatEffect.Activate();
    }

    public virtual void OnDie()
    {
        bulletMatEffect.Inactivate();
        eraseVfx?.Play();
        emitVfx?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public virtual void OnDamage(float damageRatio, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        bulletMatEffect.DamageFlash(damageRatio);
        hitVfx?.Play();
    }

    public void OnDestroy() => bulletMatEffect.KillAllTweens();
}
