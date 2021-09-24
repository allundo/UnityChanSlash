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
        ui.ResetSize(1.5f);
        shrink = ui.Resize(1.0f, 0.4f).Play();

        alphaTween?.Kill();
        fade.SetAlpha(1f, false);
        alphaTween = fade.ToAlpha(0.4f, 0.4f).Play();
    }
}
