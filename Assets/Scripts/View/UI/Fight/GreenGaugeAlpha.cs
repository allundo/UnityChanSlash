using UnityEngine;

public class GreenGaugeAlpha : GaugeAlpha
{
    [SerializeField]
    protected Color32[] ratio =
    {
        new Color32(0xFF, 0xFF, 0x00, 0xFF),
        new Color32(0xCF, 0xE7, 0x30, 0xFF),
        new Color32(0xA1, 0xD0, 0x5E, 0xFF),
        new Color32(0x73, 0xB9, 0x8E, 0xFF),
        new Color32(0x45, 0xA2, 0xBA, 0xFF),
        new Color32(0x00, 0x80, 0xFF, 0xFF),
    };

    public override void SetGauge(float valueRatio)
    {
        fillAmount = valueRatio;
        color = GetColor(valueRatio);
    }

    protected Color GetColor(float valueRatio)
    {
        for (float compare = 5.0f; compare >= 0.0f; compare -= 1.0f)
        {
            if (valueRatio > compare / 6.0f)
            {
                Color c = ratio[(int)compare];
                return new Color(c.r, c.g, c.b, color.a);
            }
        }

        return color;
    }
}
