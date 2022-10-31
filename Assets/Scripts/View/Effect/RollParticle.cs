using UnityEngine;
using DG.Tweening;

public class RollParticle : TweenParticle
{
    [SerializeField] private Vector3 angle = default;
    [SerializeField] private ParticleSystem childVFX;

    protected override ParticleSystem GetVFX() => childVFX;
    protected override Tween GetTween(float duration)
        => transform.DOLocalRotate(angle, duration, RotateMode.FastBeyond360);
}