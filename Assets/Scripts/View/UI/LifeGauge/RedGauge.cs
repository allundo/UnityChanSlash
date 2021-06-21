using DG.Tweening;

public class RedGauge : ActionGauge
{
    protected override void SetGauge(float valueRatio)
    {
        fillAmount = valueRatio;
    }

    protected override Tween GaugeTween(float valueRatio)
    {
        return DOTween.To(
            () => fillAmount,
            value => fillAmount = value,
            valueRatio,
            1f
        ).Play();
    }
}
