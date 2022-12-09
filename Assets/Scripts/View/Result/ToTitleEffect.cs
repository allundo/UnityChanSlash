using DG.Tweening;

public class ToTitleEffect : TextEffect
{
    public Tween MoveCenter()
    {
        return DOTween.Sequence()
            .AppendCallback(() => expand.Kill())
            .Append(text.MoveY(780f, 3f))
            .Join(text.Resize(1.4f, 3f))
            .AppendInterval(2f);
    }

    public Tween Finish()
    {
        return DOTween.Sequence()
            .Join(FadeOut(1f).SetEase(Ease.InCubic))
            .Join(
                DOTween.Sequence()
                    .Append(text.Resize(1.8f, 0.5f))
                    .Append(text.ResizeY(0.1f, 0.5f))
            )
            .AppendInterval(1f);
    }
}