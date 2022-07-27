using UnityEngine;
using DG.Tweening;

public class SDSighFX : SDEmotionFX
{
    protected override Tween ActivateTween()
    {
        return DOTween.Sequence()
            .Join(fade.In(1f, 0, null, null, false))
            .Join(uiTween.MoveOffset(new Vector2(-30f, -15f), 1f).SetEase(Ease.OutCubic))
            .Join(uiTween.Resize(1.75f, 1f))
            .Append(InactivateTween())
            .SetUpdate(true)
            .AsReusable(gameObject);
    }

    protected override Tween InactivateTween()
    {
        return DOTween.Sequence()
            .AppendCallback(() => isActive = false)
            .Append(fade.Out(0.5f, 0, null, null, false).SetEase(Ease.Linear))
            .AppendCallback(() => gameObject.SetActive(false));
    }
}