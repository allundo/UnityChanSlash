using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using UniRx;

public class MapAndStatusFrame : MapAndStatusBase, IPointerDownHandler, IPointerUpHandler
{
    protected Image image;

    public IObservable<Unit> Press => pressSubject;
    private ISubject<Unit> pressSubject = new Subject<Unit>();

    protected override void Awake()
    {
        base.Awake();
        image = GetComponent<Image>();
        image.color = new Color(1, 1, 1, 0.25f);
    }

    public void SetRaycast(bool isEnable)
    {
        image.raycastTarget = isEnable;
    }

    public override void InitUISize(float landscapeSize, float portraitSize, float expandSize)
    {
        this.landscapeSize = Mathf.Round(landscapeSize * 1.01666666667f);
        this.portraitSize = Mathf.Round(portraitSize * 1.01666666667f);
        this.expandSize = Mathf.Round(expandSize * 1.033333333333f);
    }

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData)
    {
        SetRaycast(false);
        pressSubject.OnNext(Unit.Default);
    }

    public Tween ExpandTween(float duration = 1f)
    {
        return DOTween.Sequence()
            .Join(image.DOFade(0.5f, duration))
            .Join(rectTransform.DOAnchorPos(anchoredCenter, duration).SetEase(Ease.OutQuart))
            .Join(rectTransform.DOSizeDelta(new Vector2(expandSize, expandSize), duration).SetEase(Ease.OutQuart));
    }

    public Tween ShrinkTween(float duration = 0.5f)
    {
        return DOTween.Sequence()
            .Join(image.DOFade(0.25f, duration))
            .Join(rectTransform.DOAnchorPos(currentPos, duration).SetEase(Ease.OutQuart))
            .Join(rectTransform.DOSizeDelta(CurrentSize, duration).SetEase(Ease.OutQuart))
            .AppendCallback(() => SetRaycast(true));
    }

    public void HideAndShow(float duration = 0.5f)
    {
        image.raycastTarget = false;

        SwitchUI(duration * 0.5f,
            () => SwitchUI(duration * 0.5f,
                () => image.raycastTarget = true
            )
        );
    }
}
