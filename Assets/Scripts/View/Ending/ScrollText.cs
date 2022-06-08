using UnityEngine;
using DG.Tweening;
using TMPro;
public class ScrollText : UISymbol
{
    private TextMeshProUGUI tmpScroll;
    private FadeTween fadeTween;
    private float fadeDuration;
    private Vector2 startPos;

    protected override void Awake()
    {
        base.Awake();
        tmpScroll = GetComponent<TextMeshProUGUI>();
        fadeTween = new FadeTween(tmpScroll);
    }

    public override UISymbol OnSpawn(Vector3 pos, IDirection dir = null, float duration = 2f)
    {
        rectTransform.anchoredPosition = startPos = pos;
        fadeDuration = duration;
        Activate();
        fadeTween.SetAlpha(0f);
        return this;
    }

    public Sequence ScrollY(float moveY, float duration = 10f)
    {
        return DOTween.Sequence()
            .Join(fadeTween.In(fadeDuration).SetEase(Ease.Linear))
            .Join(rectTransform.DOAnchorPosY(moveY, duration).SetEase(Ease.Linear).SetRelative())
            .InsertCallback(duration - fadeDuration, () => fadeTween.Out(fadeDuration).SetEase(Ease.Linear).Play())
            .SetUpdate(false);
    }

    public ScrollText SetText(string text)
    {
        tmpScroll.text = text;
        return this;
    }
}