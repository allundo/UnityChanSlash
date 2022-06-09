using UnityEngine;
using DG.Tweening;
using TMPro;
public class ScrollText : SpawnObject<ScrollText>
{
    protected RectTransform rectTransform;
    private TextMeshProUGUI tmpScroll;
    private FadeTween fadeTween;
    private float fadeDuration;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        tmpScroll = GetComponent<TextMeshProUGUI>();
        fadeTween = new FadeTween(tmpScroll);
    }

    public ScrollText OnSpawn(Vector2 startPos, float duration, string text, Color fontColor)
    {
        tmpScroll.text = text;
        tmpScroll.color = fontColor;

        return OnSpawn(startPos, null, duration);
    }
    public override ScrollText OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f)
    {
        rectTransform.anchoredPosition = pos;
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

    public Sequence ScrollY(float moveY, float duration = 10f, Ease easing = Ease.Linear)
    {
        return DOTween.Sequence()
            .Join(rectTransform.DOAnchorPosY(moveY, duration).SetEase(easing).SetRelative())
            .InsertCallback(duration - fadeDuration, Disappear)
            .SetUpdate(false);
    }
}