using UnityEngine;
using DG.Tweening;
using TMPro;

public class ResultAnimation : MonoBehaviour
{
    protected virtual string ValueFormat(ulong value) => value > 999 ? $"↑999" : value.ToString();

    [SerializeField] protected TextMeshProUGUI valueTxt = default;
    protected TextMeshProUGUI labelTxt;

    protected TextTween labelUI;
    protected TextTween valueUI;
    protected FadeTween labelFade;
    protected FadeTween valueFade;

    protected ulong value = 0;
    protected ulong prevValue = 0;

    protected virtual void Awake()
    {
        LoadLabelTxt();

        labelUI = new TextTween(labelTxt.gameObject);
        valueUI = new TextTween(valueTxt.gameObject);
        labelFade = new FadeTween(labelTxt.gameObject);
        valueFade = new FadeTween(valueTxt.gameObject);

        labelFade.SetAlpha(0f);
        valueFade.SetAlpha(0f);
    }

    protected virtual void LoadLabelTxt()
    {
        labelTxt = GetComponent<TextMeshProUGUI>();
    }

    public Tween LabelFadeIn(float duration = 0.4f)
    {
        return DOTween.Sequence()
            .AppendCallback(() => labelUI.ResetSize(1.5f))
            .Join(labelUI.Resize(1f, duration))
            .Join(labelFade.In(duration));
    }

    public virtual Tween ValueFadeIn(ulong addValue, float duration = 1f, float startSize = 2f)
    {
        return DOTween.Sequence()
            .AppendCallback(() =>
            {
                valueUI.ResetSize(startSize);
                prevValue = value;
                value += addValue;
            })
            .Join(valueUI.Resize(1f, duration))
            .Join(valueFade.In(duration * 0.5f, 0, null, null, false))
            .Join(DOVirtual.Int(0, (int)addValue, duration, count => UpdateDisplay(count)));
    }

    public virtual Tween Centering(float vecX, float duration)
    {
        return labelUI.MoveX(vecX, duration);
    }

    protected virtual void UpdateDisplay(int count)
    {
        valueTxt.text = ValueFormat(prevValue + (ulong)count);
    }
}
