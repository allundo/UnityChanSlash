using UnityEngine;
using DG.Tweening;
using TMPro;

public class WagesAnimation : ResultAnimation
{
    protected override string ValueFormat(ulong value) => $"￥{value:#,0}";

    [SerializeField] private TextMeshProUGUI wagesTxt = default;
    [SerializeField] private Color addEffectColor = default;
    [SerializeField] private AudioSource countUpSnd = default;
    [SerializeField] private AudioSource getWageSnd = default;

    protected Tween valueTween;
    protected Tween prevTween;

    private Vector2 valueTxtPos;

    private ScrollTextGenerator effectGenerator;

    protected override void LoadLabelTxt()
    {
        labelTxt = wagesTxt;
        effectGenerator = GetComponent<ScrollTextGenerator>();
        valueTxtPos = valueTxt.GetComponent<RectTransform>().anchoredPosition;
    }

    public Tween AddValue(int addValue, float duration = 0.5f)
    {
        if (addValue == 0) return null;

        valueTween?.Complete(true);

        Tween moneyEffect = effectGenerator.Spawn(valueTxtPos, 0.3f, "+" + addValue, addEffectColor)
            .ScrollY(150f, 0.6f, Ease.OutCubic)
            .Play();

        float strength = Mathf.Min(addValue, 1000000) * 0.000001f;
        valueTween = DOTween.Sequence()
            .AppendCallback(() =>
            {
                valueUI.ResetSize(1f + strength);
                prevValue = value;
                value += (ulong)addValue;
                moneyEffect?.Play();
                getWageSnd.PlayEx();
            })
            .Join(valueUI.PunchY(strength * 100f, duration, 30))
            .Join(valueUI.Resize(1f, duration * 0.25f))
            .Join(valueFade.In(duration * 0.25f, 0, null, null, false))
            .Join(DOVirtual.Int(0, addValue, duration, count => UpdateDisplay(count)))
            .Play();

        return valueTween;
    }

    protected override void UpdateDisplay(int count)
    {
        valueTxt.text = ValueFormat(prevValue + (ulong)count);
        countUpSnd.PlayEx();
    }
}
