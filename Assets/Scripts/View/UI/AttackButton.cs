using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;

public class AttackButton : MonoBehaviour
{
    protected RectTransform rectTransform;
    protected Image image;

    protected Vector2 defaultSize;
    private Color defaultColor;

    private Tween fadeIn;
    private Tween fadeOut;
    private Tween expand;
    private Tween shrink;

    public ISubject<Unit> AttackSubject { get; protected set; } = new Subject<Unit>();

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        defaultSize = rectTransform.sizeDelta;
        defaultColor = image.color;

        fadeIn = GetActivateFadeIn(image, 0.2f);
        fadeOut = GetInactivateFadeOut(image, 0.2f);
        expand = GetResize(rectTransform, 1.5f, 0.2f);
        shrink = GetResize(rectTransform, 0.5f, 0.2f);

        GetInactivateFadeOut(image, 0.0f).SetAutoKill(true).Complete();
    }

    private Tween GetActivateFadeIn(Image image, float duration = 0.2f)
    {
        return
            DOTween.ToAlpha(
                () => image.color,
                c => image.color = c,
                image.color.a,
                duration
            )
            .OnPlay(() => gameObject.SetActive(true))
            .AsReusable(gameObject);
    }

    private Tween GetInactivateFadeOut(Image image, float duration = 0.2f)
    {
        return
            DOTween.ToAlpha(
                () => image.color,
                c => image.color = c,
                0.0f,
                duration
            )
            .OnComplete(() => gameObject.SetActive(false))
            .AsReusable(gameObject);
    }

    private Tween GetResize(RectTransform rt, float ratio = 1.5f, float duration = 0.2f)
    {
        Vector2 defaultSize = rt.sizeDelta;

        return rt.DOSizeDelta(defaultSize * ratio, duration).OnComplete(() => rt.sizeDelta = defaultSize).AsReusable(gameObject);
    }

    public void Activate(Vector2 pos, float duration = 0.2f)
    {
        fadeIn.Restart();
        SetPos(pos);
    }

    public void Release(float duration = 0.2f)
    {
        Vector2 midPosToCenter = rectTransform.anchoredPosition * 0.5f;

        rectTransform.DOAnchorPos(midPosToCenter, duration).Play();
        expand.Restart();

        Inactivate();

        AttackSubject.OnNext(Unit.Default);
    }

    public void Cancel(float duration = 0.2f)
    {
        Vector2 quarterPosToCenter = rectTransform.anchoredPosition * 0.75f;

        rectTransform.DOAnchorPos(quarterPosToCenter, duration).Play();
        shrink.Restart();

        Inactivate();
    }

    public void Inactivate()
    {
        fadeOut.Restart();
    }

    public void SetPos(Vector2 pos)
    {
        rectTransform.anchoredPosition = pos;
    }
}
