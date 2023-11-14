using UnityEngine;
using DG.Tweening;

public class MessageWindowBase : FadeEnable
{
    protected UITween uiTween;

    protected override void Awake()
    {
        fade = new FadeTween(gameObject, 1f, true);
        uiTween = new UITween(gameObject, true);

        uiTween.SetSize(new Vector2(uiTween.defaultSize.x, 0f));
    }

    public override Tween FadeIn(float duration = 1f, TweenCallback onPlay = null, TweenCallback onComplete = null, bool isContinuous = true)
    {
        return DOTween.Sequence()
                .Join(uiTween.ResizeY(1f, duration))
                .Join(base.FadeIn(duration, onPlay, onComplete, isContinuous))
                .SetUpdate(true);
    }

    public override Tween FadeOut(float duration = 1f, TweenCallback onPlay = null, TweenCallback onComplete = null, bool isContinuous = true)
    {
        return DOTween.Sequence()
                .Join(uiTween.ResizeY(0f, duration))
                .Join(base.FadeOut(duration, onPlay, onComplete, isContinuous))
                .SetUpdate(true);
    }
}
