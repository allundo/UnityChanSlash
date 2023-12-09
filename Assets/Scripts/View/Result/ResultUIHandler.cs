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

    [SerializeField] private TitleDisplay titleDisplay = default;
    [SerializeField] private ClearRecord record = default;
    [SerializeField] private RankInMessage message = default;

    private Image resultBGImage;
    private Color destBGColor;

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

        record.gameObject.SetActive(false);
        message.gameObject.SetActive(false);

        var c = resultBGImage.color;
        var src = new Vector3(c.r, c.g, c.b);
        var dst = Vector3.Lerp(src, Vector3.zero, c.a);
        destBGColor = new Color(dst.x, dst.y, dst.z, 0.75f);

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
            .AppendCallback(() => wagesAnimation.AddValue(result.mapCompBonus))
            .Join(mapComp.LabelFadeIn())
            .Join(mapComp.ValueFadeIn(result.mapComp))
            .AppendCallback(() => wagesAnimation.AddValue(result.clearTimeBonus))
            .Join(clearTime.LabelFadeIn())
            .Join(clearTime.ValueFadeIn(result.clearTimeSec))
            .AppendCallback(() => wagesAnimation.AddValue(result.defeatBonus))
            .Join(defeat.LabelFadeIn())
            .Join(defeat.ValueFadeIn(result.defeatCount))
            .AppendCallback(() => wagesAnimation.AddValue(result.levelBonus))
            .Join(level.LabelFadeIn())
            .Join(level.ValueFadeIn(result.level))
            .AppendCallback(() => wagesAnimation.AddValue(result.strengthBonus))
            .Join(strength.LabelFadeIn())
            .Join(strength.ValueFadeIn(result.strength))
            .AppendCallback(() => wagesAnimation.AddValue(result.magicBonus))
            .Join(magic.LabelFadeIn())
            .Join(magic.ValueFadeIn(result.magic))
            .Append(titleDisplay.DisplayTween(result.title, () => wagesAnimation.AddValue(result.bonusPay)))
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

    public Tween CenterResults(int clearRank, DataStoreAgent.ClearRecord clearRecord, float duration = 3f)
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
            .AppendCallback(() => PlayRankIn(clearRank, clearRecord, titleButton.Show()))
            .Append(resultBGImage.DOColor(destBGColor, duration));
    }

    public Tween PlayRankIn(int rank, DataStoreAgent.ClearRecord clearRecord, Tween showTitleButton)
    {
        record.gameObject.SetActive(true);
        record.ResetPosition(new Vector2(0f, -580f));
        record.SetValues(rank, clearRecord);
        record.SetRankEnable(false);

        // SequenceEx cannot handle a lot of callbacks so mediate it by original Sequence.
        // Maybe the implementation with deep nested callback cause stack overflow.
        var seq = DOTween.Sequence()
            .Append(record.SlideInTween())
            .Append(record.RankEffect(rank))
            .Append(record.RankPunchEffect(rank));

        if (rank > 0)
        {
            message.gameObject.SetActive(true);
            message.ResetOrientation(DeviceOrientation.Portrait);
            seq.Append(message.RankInTween());
        }

        seq.Append(showTitleButton);

        return seq.Play();
    }
}
