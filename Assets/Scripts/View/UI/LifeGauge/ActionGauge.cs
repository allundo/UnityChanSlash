using DG.Tweening;

public abstract class ActionGauge : Gauge
{
    protected Tween gaugeTween = null;

    protected abstract Tween GaugeTween(float valueRatio);

    public override void UpdateGauge(float valueRatio)
    {
        gaugeTween?.Kill();
        gaugeTween = GaugeTween(valueRatio);
    }
}
