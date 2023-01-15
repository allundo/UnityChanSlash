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

    public override void SetSize(float ratio)
    {
        base.SetSize(ratio);

        expGaugeRT.anchoredPosition = defaultGaugePos * ratio;
        expGaugeRT.sizeDelta = defaultGaugeSize * ratio;
    }
}
