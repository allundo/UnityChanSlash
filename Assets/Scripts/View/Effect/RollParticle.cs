using UnityEngine;
using DG.Tweening;

public class RollParticle : TweenParticle
{
    [SerializeField] private Vector3 angle = default;
    [SerializeField] private RotateMode mode = RotateMode.LocalAxisAdd;
    [SerializeField] private Transform rollingVFX = default;

    protected override Tween GetTween(float duration)
            => rollingVFX.DOLocalRotate(angle, duration, mode);
}