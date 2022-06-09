using UnityEngine;
using System;
using DG.Tweening;
using UniRx;

public class ResultUIHandler : MonoBehaviour
{
    [SerializeField] private WagesAnimation wagesAnimation = default;
    [SerializeField] private ToTitleButton titleButton = default;
    [SerializeField] private FadeScreen fade = default;

    [SerializeField] private ResultAnimation itemPrice = default;
    [SerializeField] private ResultAnimation mapComp = default;
    [SerializeField] private ResultAnimation clearTime = default;
    [SerializeField] private ResultAnimation defeat = default;
    [SerializeField] private ResultAnimation level = default;
    [SerializeField] private ResultAnimation strength = default;
    [SerializeField] private ResultAnimation magic = default;

    public IObservable<object> TransitSignal;

    void Awake()
    {
        TransitSignal = titleButton.OnPush.First() // ContinueWith() cannot handle duplicated click events
                .ContinueWith(_ => ToTitleEffect().OnCompleteAsObservable());
    }

    public Tween ViewResult()
    {
        fade.color = Color.black;

        return DOTween.Sequence()
            .Append(fade.FadeIn(3f))
            .Append(wagesAnimation.LabelFadeIn())
            .Append(wagesAnimation.ValueFadeIn(GameInfo.Instance.moneyAmount, 2f, 0f))
            .Join(itemPrice.ValueFadeIn(GameInfo.Instance.moneyAmount, 2f))
            .Append(wagesAnimation.AddValue((int)(1000000f * GameInfo.Instance.mapComp)))
            .Join(mapComp.LabelFadeIn())
            .Join(mapComp.ValueFadeIn((ulong)(GameInfo.Instance.mapComp * 10f)))
            .Append(wagesAnimation.AddValue((int)(10000f * (Mathf.Max(0, 3600 - GameInfo.Instance.clearTimeSec)))))
            .Join(clearTime.LabelFadeIn())
            .Join(clearTime.ValueFadeIn((ulong)(GameInfo.Instance.clearTimeSec)))
            .Append(wagesAnimation.AddValue((int)(1000f * GameInfo.Instance.defeatCount)))
            .Join(defeat.LabelFadeIn())
            .Join(defeat.ValueFadeIn((ulong)(GameInfo.Instance.defeatCount)))
            .Append(wagesAnimation.AddValue((int)(10000f * GameInfo.Instance.level)))
            .Join(level.LabelFadeIn())
            .Join(level.ValueFadeIn((ulong)(GameInfo.Instance.level)))
            .Append(wagesAnimation.AddValue((int)(10000f * GameInfo.Instance.strength)))
            .Join(strength.LabelFadeIn())
            .Join(strength.ValueFadeIn((ulong)(GameInfo.Instance.strength)))
            .Append(wagesAnimation.AddValue((int)(10000f * GameInfo.Instance.magic)))
            .Join(magic.LabelFadeIn())
            .Join(magic.ValueFadeIn((ulong)(GameInfo.Instance.magic)))
            .AppendCallback(() => titleButton.Show().Play());
    }

    private Tween ToTitleEffect()
    {
        return DOTween.Sequence()
            .Join(titleButton.Apply(1f))
            .Join(fade.FadeOut(3f, 0.5f));
    }
}
