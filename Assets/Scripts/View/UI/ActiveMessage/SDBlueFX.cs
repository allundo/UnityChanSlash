using DG.Tweening;

public class SDBlueFX : SDEmotionFX
{
    protected override Tween ActivateTween()
    {
        return DOTween.Sequence()
            .Join(fade.In(1f, 0, null, null, false))
            .Join(DOTween.Sequence().Append(uiTween.ResizeY(1.5f, 0.25f)).Append(uiTween.ResizeY(1f, 1.25f)))
            .Join(uiTween.MoveY(-22f, 1.5f))
            .AppendInterval(2f)
            .Append(InactivateTween())
            .SetUpdate(true)
            .AsReusable(gameObject);
    }

    protected override Tween InactivateTween()
    {
        return DOTween.Sequence()
            .AppendCallback(() => isActive = false)
            .Append(fade.Out(0.5f, 0, null, null, false))
            .Join(uiTween.MoveY(16f, 0.5f).SetEase(Ease.InQuad))
            .Join(uiTween.ResizeY(0f, 0.5f).SetEase(Ease.InQuad))
            .AppendCallback(() => gameObject.SetActive(false));
    }
}