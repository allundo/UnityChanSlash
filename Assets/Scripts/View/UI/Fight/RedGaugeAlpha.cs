using DG.Tweening;
using UnityEngine;

public class RedGaugeAlpha : GaugeAlpha
{
    protected float prevRatio;
    protected RectTransform rectTransform;

    protected Vector3 DestRotate(float valueRatio) => new Vector3(0f, 0f, -(1.0f - valueRatio) * 360.0f);

    protected override void Awake()
    {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
    }

    public override void SetGauge(float valueRatio)
    {
        prevRatio = valueRatio;
        fillAmount = 0f;
        rectTransform.Rotate(DestRotate(valueRatio));
    }

    protected override Tween UpdateTween(float valueRatio)
    {
        fillAmount += prevRatio - valueRatio;

        Tween reduction = DOTween.To(() => fillAmount, value => fillAmount = value, 0f, 1f);
        Tween rotate = rectTransform.DORotate(DestRotate(valueRatio), 1f);

        prevRatio = valueRatio;
        return DOTween.Sequence().Join(reduction).Join(rotate).Play();
    }
}
