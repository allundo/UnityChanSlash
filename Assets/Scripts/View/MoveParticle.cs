using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(ParticleSystem))]
public class MoveParticle : MonoBehaviour
{
    [SerializeField] private float distance = 0f;
    [SerializeField] private float duration = 1f;
    private ParticleSystem vfx;
    private Tween moveTween;

    void Awake()
    {
        vfx = GetComponent<ParticleSystem>();
        Vector3 localForward = transform.localRotation * Vector3.forward;
        moveTween = transform.DOLocalMove(transform.localPosition + localForward * distance, duration)
            .SetEase(Ease.Linear)
            .AsReusable(gameObject);
    }

    public void Play()
    {
        moveTween.Restart();
        vfx.PlayEx();
    }
    private void Stop(bool withChildren = true, ParticleSystemStopBehavior stopBehavior = ParticleSystemStopBehavior.StopEmittingAndClear)
    {
        moveTween.Complete(true);
        vfx.StopEx(withChildren, stopBehavior);
    }
    public void StopEmitting(bool withChildren = true) => Stop(withChildren, ParticleSystemStopBehavior.StopEmitting);
    public void StopAndClear(bool withChildren = true) => Stop(withChildren, ParticleSystemStopBehavior.StopEmittingAndClear);
}