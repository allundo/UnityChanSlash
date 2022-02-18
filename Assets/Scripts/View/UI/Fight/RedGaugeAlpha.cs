using DG.Tweening;
using UnityEngine;

public class RedGaugeAlpha : GaugeAlpha
{
    protected float prevRatio;
    protected RectTransform rectTransform;

    protected override void Awake()
    {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
    }

    public override void SetGauge(float valueRatio)
    {
        prevRatio = valueRatio;
        fillAmount = 0f;
        rectTransform.rotation = Quaternion.Euler(0f, 0f, -(1f - valueRatio) * 360.0f);
    }

    protected override Tween UpdateTween(float valueRatio)
    {
        fillAmount += prevRatio - valueRatio;

        Tween reduction = DOTween.To(() => fillAmount, value => fillAmount = value, 0f, 1f);

        // BUG: DOTween.DORotate() applies extra 360 rotation by minus target angle vector with FastBeyond360 option.
        // To work around it, apply rotation by delta angle with SetRelative() mode.
        Tween rotate = rectTransform.DORotate(new Vector3(0f, 0f, -fillAmount * 360f), 1f, RotateMode.FastBeyond360).SetRelative(true);

        prevRatio = valueRatio;
        return DOTween.Sequence().Join(reduction).Join(rotate).Play();
    }
}
