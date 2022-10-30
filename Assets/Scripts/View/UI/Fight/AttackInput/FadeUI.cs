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

    /// <summary>
    /// Activate the UI with fade-in
    /// </summary>
    /// <param name="duration">fade duration sec</param>
    public virtual void FadeActivate(float duration = 0.2f)
    {
        isActive = true;
        BeforeFadeIn(duration);
        fade.In(duration, 0f, null, OnCompleteFadeIn).Play();
    }

    /// <summary>
    /// Called before play fade-in
    /// </summary>
    /// <param name="fadeDuration">duration of the fade-in</param>
    protected virtual void OnFadeEnable(float fadeDuration) { }

    private void BeforeFadeIn(float fadeDuration)
    {
        fade.Enable();
        OnFadeEnable(fadeDuration);
    }

    /// <summary>
    /// Called on complete fade-in
    protected virtual void OnCompleteFadeIn() { }

    /// <summary>
    /// Called before play fade-out
    protected virtual void BeforeFadeOut() { }

    /// <summary>
    /// Called on complete fade-out
    /// </summary>
    protected virtual void OnDisable() { }

    public void Disable()
    {
        OnDisable();
        fade.Disable();
    }

    public virtual void FadeInactivate(float duration = 0.1f)
    {
        isActive = false;
        BeforeFadeOut();
        fade.Out(duration, 0f, null, Disable).Play();
    }

    public abstract void SetPointerOn();
    public abstract void SetPointerOff();
}
