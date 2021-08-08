using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeActivate : MonoBehaviour
{
    protected bool isActive = true;
    protected FadeTween fade;

    protected virtual void Awake()
    {
        fade = new FadeTween(GetComponent<Image>(), 1f);
    }

    protected virtual void Start()
    {
        Inactivate(0f);
    }

    public virtual void Activate(float duration = 1f, TweenCallback onComplete = null)
    {
        if (isActive) return;

        onComplete = onComplete ?? (() => { });

        gameObject.SetActive(true);

        fade.In(duration).OnComplete(onComplete).Play();

        isActive = true;
    }

    public virtual void Inactivate(float duration = 1f, TweenCallback onComplete = null)
    {
        if (!isActive) return;

        onComplete = onComplete ?? (() => { });

        fade.Out(duration)
            .OnComplete(() =>
            {
                onComplete();
                gameObject.SetActive(false);
            })
            .Play();

        isActive = false;
    }
}
