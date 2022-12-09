using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using DG.Tweening;

public class ToTitleButton : FadeEnable
{
    [SerializeField] private ToTitleEffect txtToTitle = default;

    public IObservable<Unit> OnPush => button.OnClickAsObservable();

    private UITween ui;
    private Button button;

    protected override void Awake()
    {
        fade = new FadeTween(gameObject, 1f);
        ui = new UITween(gameObject);

        button = GetComponent<Button>();

        Inactivate();
    }

    public Tween Show(float duration = 0.1f)
    {
        return DOTween.Sequence()
            .Join(FadeIn(duration, null, null, false))
            .Join(txtToTitle.Show(duration));
    }

    public Tween Apply(float duration = 0.1f)
    {
        return DOTween.Sequence()
            .AppendCallback(() => button.enabled = false)
            .Join(FadeOut(duration))
            .Join(ui.Resize(1.5f, duration))
            .Join(txtToTitle.MoveCenter());
    }

    public Tween TextFinish() => txtToTitle.Finish();
}
