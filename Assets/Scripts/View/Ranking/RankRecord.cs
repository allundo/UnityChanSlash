using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public abstract class RankRecord : BaseRecord
{
    [SerializeField] private TextMeshProUGUI noData = default;
    [SerializeField] private Image rankImage = default;
    private TextMeshProUGUI rank = default;
    private TextTween textTween;
    private FadeTween fade;

    protected override void Awake()
    {
        rank = rankImage.GetComponentInChildren<TextMeshProUGUI>();
        textTween = new TextTween(rank);
        fade = new FadeTween(rank);
        base.Awake();
    }

    protected override void SetFormats()
    {
        textObjects.Add(rank);
        textFormats.Add(rank => (int)rank > 0 ? "第" + rank + "位" : "ランク外");
    }

    protected override void SetActive(bool isActive)
    {
        noData.enabled = !isActive;
        rankImage.enabled = isActive;

        foreach (var obj in textObjects) obj.gameObject.SetActive(isActive);
    }

    public Tween RankEffect(int rank)
    {
        var rankRatio = rank > 0 ? 11 - rank : 0;

        var sizeRatio = 1.5f + rankRatio * 0.5f;
        var duration = 0.25f + rankRatio * 0.05f;

        return DOTween.Sequence()
            .AppendCallback(() => textTween.ResetSize(sizeRatio))
            .AppendCallback(() => fade.color = new Color(1f, 0.75f, 0f, 0f))
            .AppendCallback(() => SetRankEnable(true))
            .Join(textTween.Resize(1f + Mathf.Max((rankRatio - 1) * 0.04f, 0f), duration).SetEase(Ease.InCubic))
            .Join(fade.DOColor(Color.white, duration).SetEase(Ease.Linear));
    }

    public Tween RankPunchEffect(int rank)
    {
        var rankRatio = rank > 0 ? 11 - rank : 0;
        return DOTween.Sequence()
            .Append(textTween.PunchY(2.5f + rankRatio * 0.5f, 0.25f + rankRatio * 0.05f, 20 + rankRatio))
            .Append(textTween.Resize(1f, 0.25f));
    }

    public void SetRankEnable(bool isEnable)
    {
        rank.enabled = isEnable;
    }
}
