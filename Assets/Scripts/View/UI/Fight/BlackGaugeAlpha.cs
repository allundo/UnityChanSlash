public class BlackGaugeAlpha : GaugeAlpha
{
    public override void SetGauge(float valueRatio)
    {
        fillAmount = 1.0f - valueRatio;
    }
}
