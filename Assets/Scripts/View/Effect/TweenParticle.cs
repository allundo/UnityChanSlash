using UnityEngine;
using DG.Tweening;

public abstract class TweenParticle : MonoBehaviour
{
    [SerializeField] private float duration = 1f;
    [SerializeField] private Ease ease = Ease.Linear;

    private ParticleSystem vfx;
    private Tween tween;

    void Awake()
    {
        vfx = GetVFX();
        tween = DefaultTween;
    }

    private Tween DefaultTween => GetTween(duration).SetEase(ease).AsReusable(gameObject);
    protected abstract ParticleSystem GetVFX();
    protected abstract Tween GetTween(float duration);

    protected void JoinTween(Tween tween)
    {
        this.tween = DOTween.Sequence().Join(this.tween).Join(tween).AsReusable(gameObject);
    }

    public void Play()
    {
        tween.Restart();
        vfx.PlayEx();
    }

    private void Stop(bool withChildren = true, ParticleSystemStopBehavior stopBehavior = ParticleSystemStopBehavior.StopEmittingAndClear)
    {
        tween.Complete(true);
        vfx.StopEx(withChildren, stopBehavior);
    }

    public void StopEmitting(bool withChildren = true) => Stop(withChildren, ParticleSystemStopBehavior.StopEmitting);
    public void StopAndClear(bool withChildren = true) => Stop(withChildren, ParticleSystemStopBehavior.StopEmittingAndClear);
}