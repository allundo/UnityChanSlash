using DG.Tweening;
using UnityEngine;

public abstract class FadeUI : MonoBehaviour
{
    [SerializeField] protected float maxAlpha = 1f;
    protected FadeTween fade;
    protected bool isActive = false;

    protected virtual void Awake()
    {
        FadeInit(new FadeTween(gameObject, maxAlpha));
    }

    protected virtual void FadeInit(FadeTween fade)
    {
        this.fade = fade;
        fade.SetAlpha(0f);
        fade.Disable();
        isActive = false;
    }
    public virtual void FadeActivate(float duration = 0.2f)
    {
        isActive = true;
        BeforeFadeIn(duration);
        fade.In(duration, 0f, null, OnFadeInComplete).Play();
    }

    protected virtual void OnFadeEnable(float fadeDuration) { }
    private void BeforeFadeIn(float fadeDuration)
    {
        fade.Enable();
        OnFadeEnable(fadeDuration);
    }

    protected virtual void OnFadeInComplete() { }

    protected virtual void BeforeFadeOut() { }

    protected virtual void OnFadeDisable() { }
    private void OnFadeOutComplete()
    {
        OnFadeDisable();
        fade.Disable();
    }

    public virtual void FadeInactivate(float duration = 0.1f)
    {
        isActive = false;
        BeforeFadeOut();
        fade.Out(duration, 0f, null, OnFadeOutComplete).Play();
    }

    public abstract void SetPointerOn();
    public abstract void SetPointerOff();
}
