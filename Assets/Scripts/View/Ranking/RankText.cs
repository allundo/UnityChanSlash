using UnityEngine;
using DG.Tweening;

public class RankText : BGText
{
    [SerializeField] private AudioSource punchSnd = default;

    private TextTween textTween;
    private FadeTween fade;

    protected override void Awake()
    {
        base.Awake();
        textTween = new TextTween(textMesh);
        fade = new FadeTween(textMesh);
    }

    public Tween RankEffect(int rank)
    {
        var rankRatio = rank > 0 ? 11 - rank : 0;

        var sizeRatio = 1.5f + rankRatio * 0.5f;
        var duration = 0.25f + rankRatio * 0.05f;

        return DOTween.Sequence()
            .AppendCallback(() => textTween.ResetSize(sizeRatio))
            .AppendCallback(() => fade.color = new Color(1f, 0.75f, 0f, 0f))
            .AppendCallback(() => SetTextEnable(true))
            .Join(textTween.Resize(1f + Mathf.Max((rankRatio - 1) * 0.04f, 0f), duration).SetEase(Ease.InCubic))
            .Join(fade.DOColor(Color.white, duration).SetEase(Ease.Linear));
    }

    public Tween RankPunchEffect(int rank)
    {
        var rankRatio = rank > 0 ? 11 - rank : 0;

        punchSnd.SetPitch(1.5f - rankRatio * 0.05f);
        punchSnd.volume = 0.25f + 0.025f * rankRatio;

        return DOTween.Sequence()
            .AppendCallback(() => punchSnd.PlayEx())
            .Append(textTween.PunchY(2.5f + rankRatio * 0.5f, 0.25f + rankRatio * 0.05f, 20 + rankRatio))
            .Append(textTween.Resize(1f, 0.25f));
    }
}
