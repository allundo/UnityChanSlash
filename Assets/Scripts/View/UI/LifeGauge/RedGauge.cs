using DG.Tweening;

public class RedGauge : Gauge
{
    protected override void SetGauge(float valueRatio)
    {
        fillAmount = valueRatio;
    }

    protected override Tween GetGaugeTween(float valueRatio)
    {
        return DOTween.To(
            () => fillAmount,
            value => fillAmount = value,
            valueRatio,
            1f
        );
    }
}
