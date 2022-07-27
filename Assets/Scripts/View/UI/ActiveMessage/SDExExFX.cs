using UnityEngine;
using DG.Tweening;

public class SDExExFX : SDEmotionFX
{
    protected override Tween ActivateTween()
    {
        return DOTween.Sequence()
            .AppendInterval(0.25f)
            .Append(uiTween.Punch(new Vector2(-80f, 100f), 0.5f, 20, 0.1f))
            .Join(fade.In(0.1f, 0, null, null, false).SetEase(Ease.OutCubic))
            .AppendInterval(0.5f)
            .Append(uiTween.Resize(0.75f, 0.5f))
            .AppendInterval(1f)
            .Append(InactivateTween())
            .SetUpdate(true)
            .AsReusable(gameObject);
    }
}
