using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Coffee.UIExtensions;

public class HealEffect
{
    private SkewedImage imgLight;
    private UIParticle vfx;
    private RectTransform vfxRT;
    private float width;

    private UITween uiLight;
    private Tween effectTween = null;

    public HealEffect(SkewedImage imgLight, UIParticle vfx, float gaugeWidth = 740)
    {
        this.imgLight = imgLight;
        this.vfx = vfx;
        vfxRT = vfx.GetComponent<RectTransform>();

        width = gaugeWidth;
        uiLight = new UITween(imgLight.gameObject);
        uiLight.SetSize(new Vector2(width, uiLight.CurrentSize.y), true);
        uiLight.SetPos(new Vector2(-(width + uiLight.CurrentSize.x) * 0.5f, 0f), true);

        imgLight.fillAmount = 0f;
    }

    public void PlayEffect(float duration, float lifeRatio)
    {
        ResetEffect();

        uiLight.ResetSizeX(lifeRatio);
        vfxRT.anchoredPosition = new Vector2((lifeRatio - 0.5f) * width, 0f);

        effectTween = DOTween.Sequence()
            .Join(FillTween(1f, duration * 0.75f).SetEase(Ease.InQuad))
            .Join(uiLight.MoveX(uiLight.CurrentSize.x, duration * 0.75f).SetEase(Ease.InQuad))
            .AppendCallback(() => imgLight.fillOrigin = (int)Image.OriginHorizontal.Left)
            .AppendCallback(() => vfx?.Play())
            .Join(FillTween(0f, duration * 0.25f).SetEase(Ease.Linear))
            .Join(uiLight.MoveX(uiLight.CurrentSize.x, duration * 0.25f).SetEase(Ease.Linear))
            .Play();
    }

    private void ResetEffect()
    {
        KillEffect();

        uiLight.SetPosX(-(width + uiLight.CurrentSize.x) * 0.5f);
        imgLight.fillOrigin = (int)Image.OriginHorizontal.Right;
    }

    public void KillEffect()
    {
        imgLight.fillAmount = 0f;
        effectTween?.Kill();
    }

    private Tween FillTween(float endValue, float duration)
    {
        return DOTween.To(() => imgLight.fillAmount, value => imgLight.fillAmount = value, endValue, duration);
    }
}