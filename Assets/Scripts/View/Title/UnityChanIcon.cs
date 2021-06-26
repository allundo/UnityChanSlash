using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UniRx;

public class UnityChanIcon : MonoBehaviour
{

    private RectTransform rt;
    private Image image;
    private Vector2 defaultSize;
    private Vector2 center;

    private SelectButtons buttonParent;

    private Tween selectTween = null;

    private ISubject<Transform> finishLogoTask = new Subject<Transform>();
    public IObservable<Transform> FinishLogoTask => finishLogoTask;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        buttonParent = transform.parent.GetComponentInChildren<SelectButtons>();

        defaultSize = rt.sizeDelta;
        center = -rt.anchoredPosition;
    }

    public void LogoTween()
    {
        rt.anchoredPosition = new Vector2(0f, 1920.0f);

        DOTween.Sequence()
            .AppendInterval(0.8f)
            .Append(rt.DOAnchorPos(center, 1.0f).SetEase(Ease.InQuad))
            .Append(rt.DOAnchorPos(new Vector2(center.x, center.y - 100f), 0.1f).SetEase(Ease.OutQuad))
            .Join(rt.DOSizeDelta(new Vector2(defaultSize.x * 1.5f, defaultSize.y * 0.5f), 0.1f).SetEase(Ease.OutQuad))
            .Append(rt.DOAnchorPos(center, 0.1f).SetEase(Ease.InQuad))
            .Join(rt.DOSizeDelta(defaultSize, 0.1f).SetEase(Ease.InQuad))
            .Play();
    }

    public void ToTitle(Vector3 jumpDest)
    {
        DOTween.Sequence()
            .Append(rt.DOAnchorPos(new Vector2(center.x, center.y - 100f), 0.1f).SetEase(Ease.OutQuad))
            .Join(rt.DOSizeDelta(new Vector2(defaultSize.x * 1.5f, defaultSize.y * 0.5f), 0.1f).SetEase(Ease.OutQuad))
            .Append(rt.DOAnchorPos(center, 0.1f).SetEase(Ease.InQuad))
            .Join(rt.DOSizeDelta(defaultSize, 0.1f).SetEase(Ease.InQuad))
            .Append(rt.DOJump(new Vector3(-360f, -240f), 1000f, 1, 0.6f).SetRelative())
            .Join(rt.DOSizeDelta(defaultSize * 0.3f, 0.6f).SetEase(Ease.Linear))
            .Join(rt.DORotate(new Vector3(0f, 0f, 720f), 0.6f, RotateMode.FastBeyond360).SetEase(Ease.Linear))
            .AppendInterval(0.2f)
            .AppendCallback(() => finishLogoTask.OnNext(transform))
            .Play();
    }

    public Tween SelectTween(Vector2 buttonPos)
    {
        selectTween?.Kill();

        Vector2 pos = new Vector2(buttonPos.x - 320f, buttonPos.y);
        rt.anchoredPosition = pos;

        selectTween =
            DOTween.Sequence()
                .Append(rt.DOAnchorPos(new Vector2(pos.x, pos.y - 30f), 0f).SetEase(Ease.OutQuad))
                .Join(rt.DOSizeDelta(new Vector2(defaultSize.x * 0.45f, defaultSize.y * 0.15f), 0f))
                .Append(rt.DOAnchorPos(pos, 0.1f).SetEase(Ease.OutQuad))
                .Join(rt.DOSizeDelta(defaultSize * 0.3f, 0.1f))
                .Append(rt.DOAnchorPos(new Vector2(pos.x, pos.y + 100f), 0.5f).SetEase(Ease.OutQuad))
                .SetLoops(-1, LoopType.Yoyo)
                .AsReusable(gameObject)
                .Play();

        return selectTween;

    }

}
