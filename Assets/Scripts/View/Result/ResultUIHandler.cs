using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using UniRx;

public class ResultUIHandler : MonoBehaviour
{
    [SerializeField] private WagesAnimation wagesAnimation = default;
    [SerializeField] private ToTitleButton titleButton = default;
    [SerializeField] private FadeScreen fade = default;

    public IObservable<object> TransitSignal;

    void Awake()
    {

        TransitSignal = titleButton.OnPush.First() // ContinueWith() cannot handle duplicated click events
                .ContinueWith(_ => ToTitleEffect().OnCompleteAsObservable());

        fade.SetAlpha(1f);
    }

    public Tween ViewResult()
    {
        return DOTween.Sequence()
            .Append(fade.FadeIn(3f))
            .Append(wagesAnimation.WagesFadeIn())
            .Append(wagesAnimation.ValueZoomIn(GameInfo.Instance.moneyAmount))
            .Append(wagesAnimation.AddValue(5000))
            .AppendCallback(() => titleButton.Show().Play());
    }

    private Tween ToTitleEffect()
    {
        return DOTween.Sequence()
            .Join(titleButton.Apply(1f))
            .Join(fade.FadeOut(3f, 0.5f));
    }
}
