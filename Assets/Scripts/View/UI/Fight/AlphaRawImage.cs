using UnityEngine;
using UnityEngine.UI;

public class AlphaRawImage : MonoBehaviour
{
    [SerializeField]
    protected Color32[] ratio =
    {
        new Color32(0xFF, 0xFF, 0x80, 0xFF),
        new Color32(0xDC, 0xED, 0xA3, 0xFF),
        new Color32(0xC5, 0xE2, 0xBA, 0xFF),
        new Color32(0xAE, 0xD6, 0xD1, 0xFF),
        new Color32(0x97, 0xCB, 0xE8, 0xFF),
        new Color32(0x80, 0xC0, 0xFF, 0xFF),
    };

    [SerializeField] protected float maxAlpha = 1.0f;

    [SerializeField] protected UIType uiType = UIType.None;

    protected float uiAlpha = 1f;

    protected RawImage gauge = default;

    void Awake()
    {
        uiAlpha = DataStoreAgent.Instance.GetSettingData(uiType);
        gauge = GetComponent<RawImage>();
    }

    public void SetAlpha(float alpha)
    {
        Color c = gauge.color;
        gauge.color = new Color(c.r, c.g, c.b, alpha * uiAlpha * maxAlpha);
    }

    public void SetGauge(float valueRatio)
    {
        gauge.color = GetColor(valueRatio);
    }

    protected Color GetColor(float valueRatio)
    {
        for (float compare = 5.0f; compare >= 0.0f; compare -= 1.0f)
        {
            if (valueRatio > compare / 6.0f)
            {
                Color c = ratio[(int)compare];
                return new Color(c.r, c.g, c.b, gauge.color.a);
            }
        }

        return gauge.color;
    }
}
