using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UniRx;

public class UnityChanIcon : MonoBehaviour
{
    private Tween selectTween = null;

    private ISubject<Transform> finishLogoTask = new Subject<Transform>();
    public IObservable<Transform> FinishLogoTask => finishLogoTask;

    private UITween uiTween;

    void Awake()
    {
        uiTween = new UITween(gameObject);
        uiTween.SetPos(-uiTween.CurrentPos, true);
    }

    public Tween LogoTween()
    {
        return
            DOTween.Sequence()
                .AppendCallback(() => uiTween.SetPos(new Vector2(0f, 1920.0f)))
                .AppendInterval(0.8f)
                .Append(uiTween.MoveBack(1f).SetEase(Ease.InQuad))
                .Append(uiTween.MoveY(-100f, 0.1f).SetEase(Ease.OutQuad))
                .Join(uiTween.Resize(new Vector2(1.5f, 0.5f), 0.1f).SetEase(Ease.OutQuad))
                .Append(uiTween.MoveBack(0.1f).SetEase(Ease.InQuad))
                .Join(uiTween.Resize(1f, 0.1f).SetEase(Ease.InQuad));
    }

    public void ToTitle(Vector3 jumpDest)
    {
        DOTween.Sequence()
            .Append(uiTween.MoveY(-100f, 0.1f).SetEase(Ease.OutQuad))
            .Join(uiTween.Resize(new Vector2(1.5f, 0.5f), 0.1f).SetEase(Ease.OutQuad))
            .Append(uiTween.MoveBack(0.1f).SetEase(Ease.InQuad))
            .Join(uiTween.Resize(1f, 0.1f).SetEase(Ease.InQuad))
            .Append(uiTween.Jump(-360f, -240f, 0.6f).SetRelative(true))
            .Join(uiTween.Resize(0.3f, 0.6f).SetEase(Ease.Linear))
            .Join(uiTween.Rotate(720f, 0.6f).SetEase(Ease.Linear))
            .AppendInterval(0.2f)
            .AppendCallback(() => finishLogoTask.OnNext(transform))
            .Play();
    }

    public Tween SelectTween(Vector2 iconPos)
    {
        uiTween.SetPos(iconPos, true);

        selectTween?.Kill();

        selectTween =
            DOTween.Sequence()
                .Append(uiTween.MoveY(-30f, 0.01f).SetEase(Ease.OutQuad))
                .Join(uiTween.Resize(new Vector2(0.45f, 0.15f), 0.01f))
                .Append(uiTween.MoveBack(0.05f).SetEase(Ease.OutQuad))
                .Join(uiTween.Resize(0.3f, 0.05f))
                .Append(uiTween.MoveY(100f, 0.5f).SetEase(Ease.OutQuad))
                .SetLoops(-1, LoopType.Yoyo)
                .AsReusable(gameObject)
                .Play();

        return selectTween;
    }
}
