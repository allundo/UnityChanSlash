using DG.Tweening;
using UnityEngine;

public class PanelRedGauge : PanelGauge
{
    protected RectTransform rectTransform;
    protected float currentRatio;
    protected float width;
    protected float offsetX;

    protected override void Awake()
    {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
        width = rectTransform.sizeDelta.x;
        offsetX = rectTransform.anchoredPosition.x;
    }

    public override void SetGauge(float valueRatio)
    {
        gaugeTween?.Kill();
        fillAmount = 0f;
        currentRatio = valueRatio;
    }

    private void SetXAndWidth(float valueRatio)
    {
        rectTransform.anchoredPosition = new Vector2(offsetX - width * valueRatio, rectTransform.anchoredPosition.y);
        fillAmount = Mathf.Max(0f, currentRatio - valueRatio);
    }

    protected override Tween UpdateTween(float valueRatio)
    {
        return DOTween.Sequence()
            .AppendCallback(() => SetXAndWidth(valueRatio))
            .Join(DOTween.To(() => fillAmount, value => fillAmount = value, 0, 1f))
            .Join(DOTween.To(() => currentRatio, value => currentRatio = value, valueRatio, 1f))
            .Play();
    }
}
