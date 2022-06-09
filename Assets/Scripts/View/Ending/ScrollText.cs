using UnityEngine;
using DG.Tweening;
using TMPro;
public class ScrollText : UISymbol
{
    private TextMeshProUGUI tmpScroll;
    private FadeTween fadeTween;
    private float fadeDuration;

    protected override void Awake()
    {
        base.Awake();
        tmpScroll = GetComponent<TextMeshProUGUI>();
        fadeTween = new FadeTween(tmpScroll);
    }

    public ScrollText OnSpawn(Vector2 startPos, float duration, string text, Color fontColor)
    {
        tmpScroll.text = text;
        tmpScroll.color = fontColor;

        rectTransform.anchoredPosition = startPos;
        fadeDuration = duration;
        Activate();

        return this;
    }

    public override void Activate()
    {
        gameObject.SetActive(true);
        fadeTween.SetAlpha(0f);
        fadeTween.In(fadeDuration).SetEase(Ease.Linear).Play();
    }

    private void Disappear()
    {
        fadeTween.Out(fadeDuration, 0, null, Inactivate).SetEase(Ease.Linear).Play();
    }

    public Sequence ScrollY(float moveY, float duration = 10f)
    {
        return DOTween.Sequence()
            .Join(rectTransform.DOAnchorPosY(moveY, duration).SetEase(Ease.Linear).SetRelative())
            .InsertCallback(duration - fadeDuration, Disappear)
            .SetUpdate(false);
    }
}