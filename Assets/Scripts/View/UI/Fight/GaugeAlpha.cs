using UnityEngine;

public abstract class GaugeAlpha : Gauge
{
    [SerializeField] protected float maxAlpha = 1.0f;
    [SerializeField] protected UIType uiType = UIType.None;

    protected float uiAlpha = 1f;
    protected override void Awake()
    {
        uiAlpha = DataStoreAgent.Instance.GetSettingData(uiType);
        base.Awake();
    }

    public void SetAlpha(float alpha)
    {
        Color c = color;
        color = new Color(c.r, c.g, c.b, alpha * uiAlpha * maxAlpha);
    }
}
