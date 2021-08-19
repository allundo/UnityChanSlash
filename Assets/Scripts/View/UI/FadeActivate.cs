using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeActivate : MonoBehaviour
{
    protected bool isActive = true;
    protected FadeTween fade;

    protected virtual void Awake()
    {
        fade = new FadeTween(GetComponent<MaskableGraphic>(), 1f);
    }

    protected virtual void Start()
    {
        Inactivate(0f);
    }

    public virtual Tween Activate(float duration = 1f, TweenCallback onComplete = null)
    {
        if (isActive) return null;

        onComplete = onComplete ?? (() => { });

        gameObject.SetActive(true);

        isActive = true;

        return fade.In(duration).OnComplete(onComplete).Play();
    }

    public virtual Tween Inactivate(float duration = 1f, TweenCallback onComplete = null)
    {
        if (!isActive) return null;

        onComplete = onComplete ?? (() => { });

        isActive = false;

        return fade.Out(duration)
            .OnComplete(() =>
            {
                onComplete();
                gameObject.SetActive(false);
            })
            .Play();
    }
}
