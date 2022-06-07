using UnityEngine;
using DG.Tweening;
using TMPro;

public class WagesAnimation : ResultAnimation
{
    protected override string ValueFormat(ulong value) => $"￥{value:#,0}";

    [SerializeField] private TextMeshProUGUI wagesTxt = default;

    protected override void LoadLabelTxt()
    {
        labelTxt = wagesTxt;
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
}
