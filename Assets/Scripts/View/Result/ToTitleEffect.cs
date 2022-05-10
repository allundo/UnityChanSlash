using DG.Tweening;

public class ToTitleEffect : TextEffect
{
    public Tween Finish()
    {
        return DOTween.Sequence()
            .AppendCallback(() => expand.Kill())
            .Append(text.MoveY(780f, 3f))
            .Join(text.Resize(1.5f, 3f))
            .AppendInterval(1f)
            .Join(FadeOut(1f).SetEase(Ease.InCubic))
            .Join(
                DOTween.Sequence()
                    .Join(text.Resize(2f, 0.5f))
                    .Append(text.ResizeY(0.1f, 0.5f))
            )
            .AppendInterval(1f);
    }
}