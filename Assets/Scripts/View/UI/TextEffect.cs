using DG.Tweening;

public class TextEffect : FadeEnable
{
    protected TextTween text;
    protected Tween expand;

    protected override void Awake()
    {
        fade = new FadeTween(gameObject, 1f);
        text = new TextTween(gameObject);

        expand = ExpandLoop();

        Inactivate();
    }

    public Tween Show(float duration = 0.1f)
    {
        return DOTween.Sequence()
            .Append(FadeIn(duration))
            .AppendCallback(() => expand.Restart());
    }
    public Tween Hide(float duration = 0.1f)
    {
        return DOTween.Sequence()
            .AppendCallback(() => expand.Pause())
            .Join(FadeOut(duration, null, null, false));
    }

    public Tween Apply(float duration = 0.1f)
    {
        return DOTween.Sequence()
            .Join(Hide(duration))
            .Join(text.Resize(2f, duration));
    }

    protected Tween ExpandLoop(float ratio = 1.2f, float duration = 1f)
    {
        return DOTween.Sequence()
            .Append(text.Resize(ratio, duration * 0.5f).SetEase(Ease.OutQuad))
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true)
            .AsReusable(this.gameObject);
    }
}
