using UnityEngine;
using System;
using DG.Tweening;
using UniRx;

public class ResultUIHandler : MonoBehaviour
{
    [SerializeField] private WagesAnimation wagesAnimation = default;
    [SerializeField] private ToTitleButton titleButton = default;
    [SerializeField] private FadeScreen fade = default;

    [SerializeField] private RectTransform resultsTf = default;

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

    public IObservable<Unit> ViewResult(ResultBonus result)
    {
        fade.color = Color.black;

        return DOTween.Sequence()
            .Append(fade.FadeIn(3f))
            .Append(wagesAnimation.LabelFadeIn())
            .Append(wagesAnimation.ValueFadeIn(result.itemPrice, 2f, 0f))
            .Join(itemPrice.ValueFadeIn(result.itemPrice, 2f))
            .AppendCallback(() => wagesAnimation.AddValue(result.mapCompBonus)?.Play())
            .Join(mapComp.LabelFadeIn())
            .Join(mapComp.ValueFadeIn(result.mapComp))
            .AppendCallback(() => wagesAnimation.AddValue(result.clearTimeBonus)?.Play())
            .Join(clearTime.LabelFadeIn())
            .Join(clearTime.ValueFadeIn(result.clearTimeSec))
            .AppendCallback(() => wagesAnimation.AddValue(result.defeatBonus)?.Play())
            .Join(defeat.LabelFadeIn())
            .Join(defeat.ValueFadeIn(result.defeatCount))
            .AppendCallback(() => wagesAnimation.AddValue(result.levelBonus)?.Play())
            .Join(level.LabelFadeIn())
            .Join(level.ValueFadeIn(result.level))
            .AppendCallback(() => wagesAnimation.AddValue(result.strengthBonus)?.Play())
            .Join(strength.LabelFadeIn())
            .Join(strength.ValueFadeIn(result.strength))
            .AppendCallback(() => wagesAnimation.AddValue(result.magicBonus)?.Play())
            .Join(magic.LabelFadeIn())
            .Join(magic.ValueFadeIn(result.magic))
            .AppendInterval(2f)
            .OnCompleteAsObservable(Unit.Default);
    }

    private Tween ToTitleEffect()
    {
        return DOTween.Sequence()
            .Join(titleButton.Apply(1f))
            .Join(fade.FadeOut(3f, 0.5f));
    }

    public Tween SweepResults(float duration)
    {
        return resultsTf.DOAnchorPosX(1080f, duration).SetRelative(true);
    }

    public Tween CenterResults(float duration)
    {
        return DOTween.Sequence()
            .Join(resultsTf.DOAnchorPosX(0f, duration))
            .Join(itemPrice.Centering(0f, duration))
            .Join(mapComp.Centering(0f, duration))
            .Join(clearTime.Centering(0f, duration))
            .Join(defeat.Centering(0f, duration))
            .Join(level.Centering(0f, duration))
            .Join(strength.Centering(0f, duration))
            .Join(magic.Centering(0f, duration))
            .AppendCallback(() => titleButton.Show().Play());
    }
}
