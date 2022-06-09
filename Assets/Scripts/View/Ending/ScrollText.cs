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

    public ScrollText SetText(string text)
    {
        tmpScroll.text = text;
        return this;
    }
}