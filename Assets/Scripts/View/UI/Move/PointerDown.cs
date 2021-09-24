using UnityEngine.EventSystems;
using DG.Tweening;
using UniRx;

public class PointerDown : MoveButton, IPointerDownHandler
{
    public ISubject<Unit> PressSubject { get; private set; } = new Subject<Unit>();

    public void OnPointerDown(PointerEventData eventData)
    {
        ReleaseButton();
    }

    public override void ReleaseButton()
    {
        PressSubject.OnNext(Unit.Default);

        shrink?.Kill();
        shrink = DOTween.Sequence()
            .Append(ui.Resize(1.5f, 0.05f))
            .Append(ui.Resize(1.0f, 0.4f))
            .Play();

        alphaTween?.Kill();

        alphaTween = DOTween.Sequence()
            .Append(fade.ToAlpha(1f, 0.05f))
            .Append(fade.ToAlpha(0.4f, 0.4f))
            .Play();
    }
}
