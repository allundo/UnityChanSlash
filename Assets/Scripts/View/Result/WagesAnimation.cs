using UnityEngine;
using DG.Tweening;
using System;
using TMPro;

public class WagesAnimation : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI wagesTxt = default;
    [SerializeField] private TextMeshProUGUI valueTxt = default;

    private TextTween wagesUI;
    private TextTween valueUI;
    private FadeTween wagesFade;
    private FadeTween valueFade;

    private ulong value = 0;
    private ulong prevValue = 0;
    private Tween valueTween;

    void Awake()
    {
        wagesUI = new TextTween(wagesTxt.gameObject);
        valueUI = new TextTween(valueTxt.gameObject);
        wagesFade = new FadeTween(wagesTxt.gameObject);
        valueFade = new FadeTween(valueTxt.gameObject);

        wagesFade.SetAlpha(0f);
        valueFade.SetAlpha(0f);
    }

    public Tween WagesFadeIn(float duration = 0.4f)
    {
        return DOTween.Sequence()
            .AppendCallback(() => wagesUI.ResetSize(1.5f))
            .Join(wagesUI.Resize(1f, duration))
            .Join(wagesFade.In(duration));
    }

    public Tween ValueZoomIn(int addValue, float duration = 2f)
    {
        return DOTween.Sequence()
            .AppendCallback(() =>
            {
                valueUI.ResetSize(0f);
                prevValue = value;
                value += (ulong)addValue;
            })
            .Join(valueUI.Resize(1f, duration))
            .Join(valueFade.In(duration * 0.5f, 0, null, null, false))
            .Join(DOVirtual.Int(0, addValue, duration, count => UpdateDisplay(count)));
    }

    public Tween AddValue(int addValue, float duration = 0.5f)
    {
        valueTween?.Complete(true);

        float strength = Mathf.Min(addValue, 10000) * 0.0001f;
        valueTween = DOTween.Sequence()
            .AppendCallback(() =>
            {
                valueUI.ResetSize(1f + strength);
                prevValue = value;
                value += (ulong)addValue;
            })
            .Join(valueUI.PunchY(strength * 100f, duration, 30))
            .Join(valueUI.Resize(1f, duration * 0.25f))
            .Join(valueFade.In(duration * 0.25f, 0, null, null, false))
            .Join(DOVirtual.Int(0, addValue, duration, count => UpdateDisplay(count)));

        return valueTween;
    }

    private void UpdateDisplay(int count)
    {
        valueTxt.text = String.Format("￥{0:#,0}", prevValue + (ulong)count);
    }
}
