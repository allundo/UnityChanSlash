using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class StatusGauge : StatusContent
{
    [SerializeField] private RawImage expGauge;
    private Image expBar;

    private RectTransform expGaugeRT;

    private Vector2 defaultGaugePos;
    private Vector2 defaultGaugeSize;

    protected override void Awake()
    {
        base.Awake();
        expBar = value as Image;

        expGaugeRT = expGauge.GetComponent<RectTransform>();
        defaultGaugePos = expGaugeRT.anchoredPosition;
        defaultGaugeSize = expGaugeRT.sizeDelta;
    }

    public override void SetValue(float fillRatio)
    {
        expBar.fillAmount = fillRatio;
    }

    protected override void SetSizeAndPos(float ratio, Vector2 contentPos)
    {
        base.SetSizeAndPos(ratio, contentPos);

        expGaugeRT.anchoredPosition = defaultGaugePos * ratio;
        expGaugeRT.sizeDelta = defaultGaugeSize * ratio;
    }
    protected override void SetAlpha(float alpha)
    {
        base.SetAlpha(alpha);
        expBar.color = new Color(expBar.color.r, expBar.color.b, expBar.color.g, alpha);
        expGauge.color = new Color(expGauge.color.r, expGauge.color.b, expGauge.color.g, alpha);
    }
}
