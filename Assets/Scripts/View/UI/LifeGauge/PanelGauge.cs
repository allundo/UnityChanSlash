using DG.Tweening;
using UnityEngine.UI;

public class PanelGauge : FadeUI
{
    protected Image gauge;
    public float fillAmount
    {
        get
        {
            return gauge.fillAmount;
        }
        set
        {
            gauge.fillAmount = value;
        }
    }

    protected Tween gaugeTween = null;

    protected virtual Tween UpdateTween(float valueRatio) => null;

    protected override void Awake()
    {
        base.Awake();
        gauge = GetComponent<Image>();
        SetGauge(1.0f);
    }

    public virtual void SetGauge(float valueRatio)
    {
        fillAmount = valueRatio;
    }

    public virtual void UpdateGauge(float valueRatio)
    {
        gaugeTween?.Kill();

        gaugeTween = UpdateTween(valueRatio);
    }
}
