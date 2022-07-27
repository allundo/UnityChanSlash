using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public abstract class SDEmotionFX : MonoBehaviour
{
    protected static Tween prevTween = null;
    protected FadeTween fade;
    protected UITween uiTween;
    private Image image;

    protected bool isActive = false;

    protected Tween activateTween;

    void Awake()
    {
        image = GetComponent<Image>();
        fade = new FadeTween(image, 1f, true);
        uiTween = new UITween(gameObject, true);

        fade.SetAlpha(0f);

        gameObject.SetActive(false);

        activateTween = ActivateTween();
    }

    protected abstract Tween ActivateTween();

    protected virtual Tween InactivateTween()
    {
        return DOTween.Sequence()
            .AppendCallback(() => isActive = false)
            .Append(fade.Out(0.25f, 0, null, null, false))
            .AppendCallback(() => gameObject.SetActive(false));
    }

    public void Activate()
    {
        prevTween?.Complete();

        gameObject.SetActive(true);

        uiTween.ResetPos();

        activateTween.Restart();

        prevTween = activateTween;

        isActive = true;
    }

    public void Inactivate()
    {
        if (!isActive) return;
        prevTween?.Pause();
        InactivateTween().Play();
    }
}