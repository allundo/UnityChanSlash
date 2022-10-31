using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(ParticleSystem))]
public class MoveParticle : TweenParticle
{
    [SerializeField] private float distance = 0f;

    protected override ParticleSystem GetVFX() => GetComponent<ParticleSystem>();
    protected override Tween GetTween(float duration)
    {
        Vector3 localForward = transform.localRotation * Vector3.forward;
        return transform.DOLocalMove(transform.localPosition + localForward * distance, duration);
    }
}