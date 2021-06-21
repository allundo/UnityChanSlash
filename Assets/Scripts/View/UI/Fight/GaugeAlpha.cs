using UnityEngine;

public abstract class GaugeAlpha : Gauge
{
    [SerializeField] protected float maxAlpha = 1.0f;
    public void SetAlpha(float alpha)
    {
        Color c = color;
        color = new Color(c.r, c.g, c.b, alpha * maxAlpha);
    }
}
