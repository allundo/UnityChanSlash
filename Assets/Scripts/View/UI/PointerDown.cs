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
        defaultAlpha?.Kill();
        Resize(1.5f);
        SetAlpha(1.0f);
        shrink = GetResize(1.0f, 0.4f).Play();
        defaultAlpha = GetToAlpha(0.4f, 0.4f).Play();
    }
}
