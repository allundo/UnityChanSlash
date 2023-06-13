using UnityEngine;
using DG.Tweening;

public class BulletEffect : MagicEffect
{
    [SerializeField] protected ParticleSystem emitVfx = default;
    [SerializeField] protected ParticleSystem fireVfx = default;
    [SerializeField] protected ParticleSystem hitVfx = default;
    [SerializeField] protected ParticleSystem eraseVfx = default;

    [SerializeField] protected AudioSource fireSound = default;

    [SerializeField] protected Transform meshTf = null;
    [SerializeField] protected float dyingFXDuration = 0f;
    [SerializeField] protected float cycleTimeSec = 0f;
    [SerializeField] protected Color blinkColor = default;

    protected MatColorEffect bulletMatEffect = null;

    protected virtual void Awake()
    {
        bulletMatEffect = new BulletMatEffect(meshTf, dyingFXDuration, cycleTimeSec, blinkColor);
    }

    protected virtual Tween RollingTween(float duration = 0.25f)
    {
        return meshTf.DOLocalRotate(new Vector3(0f, 0f, 90f), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetRelative()
            .SetLoops(-1, LoopType.Incremental);
    }

    public override void Disappear(TweenCallback onComplete = null, float duration = 0.5f)
    {
        OnDisappear();
        bulletMatEffect.Inactivate(onComplete, duration);
    }

    protected virtual void OnDisappear()
    {
        eraseVfx.PlayEx();
        emitVfx.StopEmitting();
    }

    public override void OnActive()
    {
        fireSound.PlayEx();
        emitVfx.PlayEx();
        fireVfx.PlayEx();
        bulletMatEffect.Activate();
    }

    public override void OnHit()
    {
        bulletMatEffect.DamageFlash();
        hitVfx?.Play();
    }

    public override void OnDestroyByReactor() => bulletMatEffect.KillAllTweens();
}
