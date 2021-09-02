using UnityEngine;
using DG.Tweening;

public class FadeActivate : MonoBehaviour
{
    protected bool isActive = true;
    protected FadeTween fade;

    protected virtual void Awake()
    {
        fade = new FadeTween(gameObject, 1f);
    }

    protected virtual void Start()
    {
        Inactivate();
    }

    public virtual void Activate()
    {
        fade.Enable();
        isActive = true;
        fade.SetAlpha(1f);
    }

    public virtual void Inactivate()
    {
        isActive = false;
        fade.SetAlpha(0f);
        fade.Disable();
    }

    public virtual Tween FadeIn(float duration = 1f, TweenCallback onPlay = null, TweenCallback onComplete = null)
    {
        onPlay = onPlay ?? (() => { });
        onComplete = onComplete ?? (() => { });

        return fade.In(duration, 0f,
            () =>
            {
                fade.Enable();
                isActive = true;
                onPlay();
            },
            onComplete);
    }

    public virtual Tween FadeOut(float duration = 1f, TweenCallback onPlay = null, TweenCallback onComplete = null)
    {
        onPlay = onPlay ?? (() => { });
        onComplete = onComplete ?? (() => { });

        return fade.Out(duration, 0f,
            () =>
            {
                isActive = false;
                onPlay();
            },
            () =>
            {
                onComplete();
                fade.Disable();
            });
    }
}
