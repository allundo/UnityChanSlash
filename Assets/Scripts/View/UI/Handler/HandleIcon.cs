using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;
using System.Linq;

[RequireComponent(typeof(Image))]
public class HandleIcon : FadeEnable
{
    [SerializeField] private FlickInteraction[] flicks = default;
    [SerializeField] private ItemInventory itemInventory = default;
    [SerializeField] private HandleText text = default;

    protected UITween ui;
    protected Tween prevTween = null;
    protected Tween applyTween = null;

    private FlickInteraction.FlickDirection currentFlick = null;
    protected ItemIcon currentItemIcon = null;

    protected override void Awake()
    {
        fade = new FadeTween(gameObject, 1f);
        ui = new UITween(gameObject);
        Inactivate();
    }

    protected virtual void Start()
    {
        Observable
            .Merge(flicks.Select(flick => flick.Drag))
            .Subscribe(flick => SetFadeActive(flick))
            .AddTo(this);

        Observable
            .Merge(flicks.Select(flick => flick.FlickSubject))
            .Subscribe(_ => PlayTween(Apply()))
            .AddTo(this);

        Observable
            .Merge(flicks.Select(flick => flick.IsReleased))
            .Subscribe(_ => Disable())
            .AddTo(this);

        itemInventory.OnPutItem
            .Subscribe(itemIcon => SetItemIconActive(itemIcon))
            .AddTo(this);

        itemInventory.OnPutApply
            .Subscribe(_ => PlayTween(ApplyPut()))
            .AddTo(this);
    }

    public void Activate(Sprite sprite)
    {
        if (isActive) return;

        fade.sprite = sprite;
        base.Activate();
    }

    private void SetFadeActive(FlickInteraction.FlickDirection flick)
    {
        if (flick.Ratio > 0.5f)
        {
            if (!isActive)
            {
                PlayTween(Show(flick));
            }
            else if (flick != currentFlick)
            {
                PlayTween(Switch(flick));
            }

            currentFlick = flick;
        }
        else if (isActive)
        {
            Disable();
            currentFlick = null;
        }
    }

    private Tween Show(FlickInteraction.FlickDirection flick, float duration = 0.4f)
    {
        return DOTween.Sequence()
            .AppendCallback(() => fade.sprite = flick.sprite)
            .AppendCallback(() => ui.ResetSize(2f))
            .Join(base.FadeIn(duration))
            .Join(ui.Resize(1.5f, duration))
            .Join(text.Show(flick.text));
    }

    private Tween Switch(FlickInteraction.FlickDirection flick, float duration = 0.4f)
    {
        return DOTween.Sequence()
            .AppendCallback(() => fade.sprite = flick.sprite)
            .AppendCallback(() => ui.ResetSize(2f))
            .Join(base.FadeIn(duration, null, null, false))
            .Join(ui.Resize(1.5f, duration))
            .Join(text.Switch(flick.text));
    }

    private Tween Hide(float duration = 0.4f)
    {
        return DOTween.Sequence()
            .Join(base.FadeOut(duration))
            .Join(ui.Resize(1f, duration))
            .Join(text.Hide());
    }
    private Tween Apply(float duration = 0.3f)
    {
        applyTween = DOTween.Sequence()
            .Join(base.FadeOut(duration, null, null, false))
            .Join(ui.Resize(4f, duration))
            .Join(text.Apply(duration));

        return applyTween;
    }

    public void Disable()
    {
        if (!isActive) return;

        PlayTween(Hide());
    }

    private void SetItemIconActive(ItemIcon itemIcon)
    {
        if (itemIcon != null)
        {
            PlayTween(ShowItemIcon(itemIcon));
        }
        else if (currentItemIcon != null)
        {
            PlayTween(ResetItemIcon(currentItemIcon));
        }

        currentItemIcon = itemIcon;
    }

    private Tween ShowItemIcon(ItemIcon itemIcon, float duration = 0.4f)
    {
        base.Activate();
        return DOTween.Sequence()
            .AppendCallback(() => fade.SetAlpha(0f))
            .AppendCallback(() => itemIcon.SetParent(transform, true, true))
            .Join(itemIcon.LocalMove(Vector2.zero, duration))
            .Join(itemIcon.Resize(2f, duration))
            .Join(text.Show("PUT"));
    }

    private Tween ResetItemIcon(ItemIcon itemIcon, float duration = 0.4f)
    {
        base.Inactivate();
        return DOTween.Sequence()
            .AppendCallback(() => itemIcon.ResetSize(1.5f))
            .AppendCallback(() => itemIcon.SetParent(itemInventory.transform, false))
            .Join(text.Hide());
    }

    private Tween ApplyPut(float duration = 0.3f)
    {
        base.Inactivate();
        currentItemIcon = null;
        return text.Apply(duration);
    }

    private Tween PlayTween(Tween tween)
    {
        // Don't cancel Apply Tween
        if (prevTween == applyTween)
        {
            tween?.Kill();
            prevTween = tween;
        }
        else
        {
            prevTween?.Kill();
            prevTween = tween.Play();
        }

        return tween;
    }
}
