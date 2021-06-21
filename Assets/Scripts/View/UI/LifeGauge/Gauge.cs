using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public abstract class Gauge : MonoBehaviour
{
    protected Image gauge = default;
    protected Tween gaugeTween = null;
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

    void Awake()
    {
        gauge = GetComponent<Image>();
    }

    void Start()
    {
        SetGauge(1.0f);
    }

    protected abstract void SetGauge(float valueRatio);
    protected abstract Tween GetGaugeTween(float valueRatio);

    public virtual void UpdateGauge(float valueRatio)
    {
        gaugeTween?.Kill();
        gaugeTween = GetGaugeTween(valueRatio)?.Play();
    }
}
