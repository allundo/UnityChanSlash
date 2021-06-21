using DG.Tweening;
using UnityEngine;

public class GaugeAlpha : Gauge
{
    [SerializeField] protected float maxAlpha = 1.0f;
    public void SetAlpha(float alpha)
    {
        Color c = gauge.color;
        gauge.color = new Color(c.r, c.g, c.b, alpha * maxAlpha);
    }

    protected override void SetGauge(float valueRatio)
    {
        throw new System.NotImplementedException();
    }

    protected override Tween GetGaugeTween(float valueRatio)
    {
        throw new System.NotImplementedException();
    }
}
