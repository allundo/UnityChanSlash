using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ResumeButton : FadeEnable, IPointerDownHandler
{
    [SerializeField] private TextEffect txtResume = default;
    [SerializeField] private RectTransform guardRegion = default;

    public UnityEvent onPush = new UnityEvent();

    private UITween ui;

    protected override void Awake()
    {
        fade = new FadeTween(gameObject, 1f);
        ui = new UITween(gameObject);

        ui.SetSize(guardRegion.sizeDelta * 1.25f, true);


        Inactivate();
    }

    public void ResetOrientation(DeviceOrientation orientation)
        => ui.SetScreenPos(new Vector2(Screen.width * 0.5f, guardRegion.position.y));

    public void OnPointerDown(PointerEventData eventData) => onPush.Invoke();

    public Tween Show(float duration = 0.1f)
    {
        return DOTween.Sequence()
            .Join(FadeIn(duration, null, null, false))
            .Join(txtResume.Show(duration));
    }

    public Tween Hide(float duration = 0.1f)
    {
        return DOTween.Sequence()
            .Join(FadeOut(duration, null, null, false))
            .Join(txtResume.Hide(duration));
    }

    protected Tween Apply(float duration = 0.1f)
    {
        return DOTween.Sequence()
            .Join(FadeOut(duration))
            .Join(ui.Resize(2f, duration))
            .Join(txtResume.Apply(duration));
    }
}
