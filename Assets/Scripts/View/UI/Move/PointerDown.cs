using UnityEngine.EventSystems;
using DG.Tweening;
using UniRx;

public class PointerDown : MoveButton, IPointerDownHandler
{
    public ISubject<Unit> PressSubject { get; private set; } = new Subject<Unit>();

#if UNITY_EDITOR
    // BUG: Input Action "Position[Pointer]" causes pointer event double firing on Editor.
    private bool isFired = false;
    private bool CanFire()
    {
        if (isFired) return false;
        isFired = true;
        Observable.NextFrame().Subscribe(_ => isFired = false);
        return true;
    }
#endif

    public void OnPointerDown(PointerEventData eventData)
    {
#if UNITY_EDITOR
        // BUG: Input Action "Position[Pointer]" causes pointer event double firing on Editor.
        if (!CanFire()) return;
#endif
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
