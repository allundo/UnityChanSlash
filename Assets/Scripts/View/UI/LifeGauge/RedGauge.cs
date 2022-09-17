using DG.Tweening;

public class RedGauge : Gauge
{
    public override void SetGauge(float valueRatio)
    {
        fillAmount = valueRatio;
    }

    protected override Tween UpdateTween(float valueRatio)
    {
        return DOTween.To(
            () => fillAmount,
            value => fillAmount = value,
            valueRatio,
            1f
        ).Play();
    }

    public override void UpdateGauge(float valueRatio, bool enableAnimation = true)
    {
        gaugeTween?.Kill();

        if (enableAnimation)
        {
            gaugeTween = UpdateTween(valueRatio);
        }
        else
        {
            SetGauge(valueRatio);
        }
    }
}
