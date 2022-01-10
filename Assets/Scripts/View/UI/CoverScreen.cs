using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CoverScreen : FadeScreen
{
    [SerializeField] protected Image coverImage = null;

    protected FadeMaterialColor fadeMC = null;

    public override Vector2 sizeDelta
    {
        get
        {
            return coverImage.rectTransform.sizeDelta;
        }
        set
        {
            if (fadeImage != null) fadeImage.rectTransform.sizeDelta = value;
            coverImage.rectTransform.sizeDelta = value;
        }
    }

    public override void SetAlpha(float alpha)
    {
        fade.SetAlpha(alpha);
        fadeMC.SetAlpha(alpha);
    }

    protected override void Awake()
    {
        if (fadeImage != null) fade = new FadeTween(fadeImage, 1f, true);

        coverImage = coverImage ?? GetComponent<Image>();
        fadeMC = new FadeMaterialColor(coverImage, 1f, true);

        SetAlpha(0f);
    }

    public Tween CoverOff(float duration = 1f, float delay = 0f)
    {
        return DOTween.Sequence()
            .AppendCallback(() => fade?.SetAlpha(0f))
            // Fade out black image to display screen
            .Append(fadeMC.Out(duration, delay).SetEase(Ease.InQuad));
    }

    public Tween CoverOn(float duration = 1f, float delay = 0f)
    {
        return DOTween.Sequence()
            .AppendCallback(() => fade?.SetAlpha(0f))
            // Fade out black image to display screen
            .Append(fadeMC.In(duration, delay).SetEase(Ease.OutQuad));
    }

    public override Tween FadeIn(float duration = 1f, float delay = 0f, bool isContinuous = true)
    {
        return DOTween.Sequence()
            .AppendCallback(() => fadeMC.SetAlpha(0f))
            .Append(base.FadeIn(duration, delay, isContinuous));
    }

    public override Tween FadeOut(float duration = 1f, float delay = 0f, bool isContinuous = true)
    {
        return DOTween.Sequence()
            .AppendCallback(() => fadeMC.SetAlpha(0f))
            .Append(base.FadeOut(duration, delay, isContinuous));
    }
}
