using UnityEngine;
using DG.Tweening;

public class SDExFX : SDEmotionFX
{
    protected override Tween ActivateTween()
    {
        return DOTween.Sequence()
            .Join(fade.In(0.25f, 0, null, null, false).SetEase(Ease.OutCubic))
            .Join(uiTween.MoveOffset(new Vector2(-40f, 50f), 0.25f).SetEase(Ease.OutQuad))
            .Join(uiTween.Resize(1.5f, 0.25f).SetEase(Ease.OutQuad))
            .Append(uiTween.MoveBack(0.25f).SetEase(Ease.InQuad))
            .Join(uiTween.Resize(1f, 0.25f).SetEase(Ease.InQuad))
            .Append(uiTween.MoveOffset(new Vector2(-20f, 25f), 0.25f).SetEase(Ease.OutQuad))
            .Join(uiTween.Resize(1.3f, 0.25f).SetEase(Ease.OutQuad))
            .Append(uiTween.MoveOffset(new Vector2(-15f, 17.5f), 0.2f).SetEase(Ease.OutQuad))
            .Join(uiTween.Resize(1.25f, 0.2f).SetEase(Ease.OutQuad))
            .AppendInterval(2.25f)
            .Append(InactivateTween())
            .SetUpdate(true)
            .AsReusable(gameObject);
    }
}
