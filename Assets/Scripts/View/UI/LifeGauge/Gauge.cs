using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public abstract class Gauge : MonoBehaviour
{
    protected Image gauge = default;
    public Color color
    {
        get
        {
            return gauge.color;
        }
        set
        {
            gauge.color = value;
        }
    }
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

    protected virtual void Awake()
    {
        gauge = GetComponent<Image>();
    }

    void Start()
    {
        SetGauge(1.0f);
    }

    public virtual void SetGauge(float valueRatio)
    {
        fillAmount = valueRatio;
    }

    public virtual void UpdateGauge(float valueRatio, bool isFlash = false)
    {
        gaugeTween?.Kill();

        if (isFlash) gauge.color = new Color(1, 1, 1);

        gaugeTween = UpdateTween(valueRatio);
    }

    public virtual void Enable() => gauge.enabled = true;
    public virtual void Disable() => gauge.enabled = false;
}
