using UnityEngine;
using DG.Tweening;

public class SDSwingFX : SDEmotionFX
{
    [SerializeField] private float swingAngle = 20f;
    [SerializeField] private float swingDuration = 0.1f;
    [SerializeField] private float maxSize = 1.5f;
    [SerializeField] private int additionalSwing = 2;

    protected override Tween ActivateTween()
    {
        var seq = DOTween.Sequence()
            .Join(fade.In(swingDuration, 0, null, null, false))
            .Join(uiTween.Rotate(swingAngle, swingDuration, false).SetEase(Ease.Linear))
            .Append(uiTween.Rotate(-swingAngle, swingDuration, false).SetEase(Ease.Linear))
            .Join(uiTween.Resize((1 + maxSize) * 0.5f, swingDuration).SetEase(Ease.Linear))
            .Append(uiTween.Rotate(swingAngle, swingDuration, false).SetEase(Ease.Linear))
            .Join(uiTween.Resize(maxSize, swingDuration).SetEase(Ease.Linear));

        for (int i = 0; i < additionalSwing; i++)
        {
            seq
                .Append(uiTween.Rotate(-swingAngle, swingDuration, false).SetEase(Ease.Linear))
                .Append(uiTween.Rotate(swingAngle, swingDuration, false).SetEase(Ease.Linear));
        }

        seq
            .Append(uiTween.Rotate(-swingAngle, swingDuration, false).SetEase(Ease.Linear))
            .Join(uiTween.Resize((1 + maxSize) * 0.5f, swingDuration).SetEase(Ease.Linear))
            .Append(uiTween.Rotate(swingAngle, swingDuration, false).SetEase(Ease.Linear))
            .Join(uiTween.Resize(1f, swingDuration).SetEase(Ease.Linear))
            .Append(uiTween.Rotate(0f, swingDuration, false).SetEase(Ease.Linear))
            .AppendInterval(Mathf.Max(0, 2f - swingDuration * 2 * additionalSwing))
            .Append(InactivateTween())
            .SetUpdate(true)
            .AsReusable(gameObject);

        return seq;
    }
}
