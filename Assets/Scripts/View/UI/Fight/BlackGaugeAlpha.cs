using DG.Tweening;

public class BlackGaugeAlpha : GaugeAlpha
{
    protected override void SetGauge(float valueRatio)
    {
        fillAmount = 1.0f - valueRatio;
    }
}
