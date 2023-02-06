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

    [SerializeField] private RectTransform resultsTf = default;

    [SerializeField] private ResultAnimation itemPrice = default;
    [SerializeField] private ResultAnimation mapComp = default;
    [SerializeField] private ResultAnimation clearTime = default;
    [SerializeField] private ResultAnimation defeat = default;
    [SerializeField] private ResultAnimation level = default;
    [SerializeField] private ResultAnimation strength = default;
    [SerializeField] private ResultAnimation magic = default;

    private Image resultBGImage;

    public IObservable<Unit> ClickToEnd { get; private set; }
    public IObservable<Unit> FadeOutScreen { get; private set; }

    public IObservable<object> TransitSignal { get; private set; }

    void Awake()
    {
        ClickToEnd = titleButton.OnPush.First();

        FadeOutScreen = ClickToEnd
            .ContinueWith(_ => ClickTitleEffect());

        TransitSignal = FadeOutScreen
            .ContinueWith(_ => titleButton.TextFinish().OnCompleteAsObservable());

        resultBGImage = resultsTf.GetComponent<Image>();
        resultBGImage.enabled = false;
    }

    public IObservable<Unit> ViewResult(ResultBonus result)
    {
        fade.color = Color.black;

        fade.FadeIn(3f);

        return DOTween.Sequence()
            .AppendInterval(3f)
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

    public IObservable<Unit> ClickTitleEffect()
    {
        return fade.FadeOutObservable(
            3f, 0.5f, Ease.OutQuad,
            titleButton.Apply(1f).OnCompleteAsObservable(Unit.Default)
        );
    }

    public Tween SweepResults(float duration)
    {
        return resultsTf.DOAnchorPosX(1080f, duration)
            .OnComplete(() => resultBGImage.enabled = true)
            .SetRelative(true);
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
