using UnityEngine;
using DG.Tweening;
using System.Linq;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class HandleText : FadeActivate
{
    private TextMeshProUGUI tm = default;

    protected UITween ui;

    protected override void Awake()
    {
        tm = GetComponent<TextMeshProUGUI>();
        fade = new FadeTween(gameObject, 0.75f);
        ui = new UITween(gameObject);
        Inactivate();
    }

    public Tween Show(string str, float duration = 0.2f)
    {
        return DOTween.Sequence()
            .AppendCallback(() => tm.text = str)
            .AppendCallback(() => ui.ResetSize(1f, 0.5f))
            .Join(base.FadeIn(duration * 0.25f))
            .Join(ui.ResizeY(1.75f, duration * 0.25f))
            .Append(ui.ResizeY(1f, duration * 0.75f));
    }

    public Tween Hide(float duration = 0.2f)
    {
        return DOTween.Sequence()
            .Join(base.FadeOut(duration))
            .Join(ui.Resize(0.5f, duration));
    }

    public Tween Apply(float duration = 0.3f)
    {
        return DOTween.Sequence()
            .AppendCallback(() => ui.ResetSize())
            .Join(base.FadeOut(duration, null, null, false))
            .Join(ui.Resize(2f, duration));
    }
}
