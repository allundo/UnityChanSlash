using DG.Tweening;
using UnityEngine;

public class GreenGauge : Gauge
{
    [SerializeField]
    protected Color32[] ratio =
    {
        new Color32(0xFF, 0xFF, 0x00, 0xFF),
        new Color32(0xa3, 0xFF, 0x00, 0xFF),
        new Color32(0x30, 0xFF, 0x00, 0xFF),
        new Color32(0x00, 0xF6, 0x3F, 0xFF),
        new Color32(0x00, 0xE9, 0x9B, 0xFF),
        new Color32(0x00, 0xE0, 0xE0, 0xFF),
    };

    public override void SetGauge(float valueRatio)
    {
        fillAmount = valueRatio;
        color = GetColor(valueRatio);
    }

    protected override Tween UpdateTween(float valueRatio)
    {
        fillAmount = valueRatio;
        return gauge.DOColor(GetColor(valueRatio), 0.5f).Play();
    }

    protected Color GetColor(float valueRatio)
    {
        for (float compare = 5.0f; compare >= 0.0f; compare -= 1.0f)
        {
            if (valueRatio > compare / 6.0f)
            {
                return ratio[(int)compare];
            }
        }

        return color;
    }
}
