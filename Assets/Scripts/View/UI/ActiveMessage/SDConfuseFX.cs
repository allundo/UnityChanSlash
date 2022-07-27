using UnityEngine;
using DG.Tweening;

public class SDConfuseFX : SDEmotionFX
{
    protected override Tween ActivateTween()
    {
        return DOTween.Sequence()
            .Join(fade.In(1f, 0, null, null, false).SetEase(Ease.Linear))
            .Join(uiTween.MoveOffset(new Vector2(-30f, 30f), 2.5f).SetEase(Ease.OutCubic))
            .Join(uiTween.Resize(2f, 1f))
            .Append(InactivateTween())
            .SetUpdate(true)
            .AsReusable(gameObject);
    }

    protected override Tween InactivateTween()
    {
        return DOTween.Sequence()
            .AppendCallback(() => isActive = false)
            .Append(fade.Out(1f, 0, null, null, false))
            .Join(uiTween.MoveOffset(new Vector2(-40f, 50f), 1f))
            .AppendCallback(() => gameObject.SetActive(false));
    }
}