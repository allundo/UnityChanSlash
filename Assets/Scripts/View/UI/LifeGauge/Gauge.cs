using UnityEngine;
using UnityEngine.UI;

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

    protected virtual void Awake()
    {
        gauge = GetComponent<Image>();
    }

    void Start()
    {
        SetGauge(1.0f);
    }

    protected abstract void SetGauge(float valueRatio);
    public virtual void UpdateGauge(float valueRatio)
    {
        SetGauge(valueRatio);
    }
}
