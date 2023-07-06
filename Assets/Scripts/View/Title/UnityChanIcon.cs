using UnityEngine;
using DG.Tweening;
using System;
using UniRx;

public class UnityChanIcon : SelectIcon
{
    [SerializeField] private AudioSource boundSnd = default;
    private ISubject<Transform> finishLogoTask = new Subject<Transform>();
    public IObservable<Transform> FinishLogoTask => finishLogoTask;

    protected override void Awake()
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
                .AppendCallback(() => boundSnd.PlayEx())
                .Append(uiTween.MoveY(-100f, 0.1f).SetEase(Ease.OutQuad))
                .Join(uiTween.Resize(new Vector2(1.5f, 0.5f), 0.1f).SetEase(Ease.OutQuad))
                .Append(uiTween.MoveBack(0.1f).SetEase(Ease.InQuad))
                .Join(uiTween.Resize(1f, 0.1f).SetEase(Ease.InQuad));
    }

    public Tween TitleTween(SelectButtons buttons)
    {
        return
            DOTween.Sequence()
                .Append(uiTween.MoveY(-100f, 0.1f).SetEase(Ease.OutQuad))
                .Join(uiTween.Resize(new Vector2(1.5f, 0.5f), 0.1f).SetEase(Ease.OutQuad))
                .Append(uiTween.MoveBack(0.1f).SetEase(Ease.InQuad))
                .Join(uiTween.Resize(1f, 0.1f).SetEase(Ease.InQuad))
                .Append(uiTween.Jump(buttons.Pos + buttons.startButton.IconPos, 0.6f).SetRelative(true))
                .Join(uiTween.Resize(0.3f, 0.6f).SetEase(Ease.Linear))
                .Join(uiTween.Rotate(720f, 0.6f).SetEase(Ease.Linear))
                .AppendInterval(0.2f)
                .AppendCallback(() => SetParent(buttons.transform));
    }

    private void SetParent(Transform parent) => transform.SetParent(parent);

    public override Tween SelectTween(Vector2 iconPos)
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
