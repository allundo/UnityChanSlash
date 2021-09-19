using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeScreen : MonoBehaviour
{
    [SerializeField] protected Image fadeImage = null;

    protected FadeTweenImage fade = null;

    public virtual Vector2 sizeDelta
    {
        get
        {
            return fadeImage.rectTransform.sizeDelta;
        }
        set
        {
            fadeImage.rectTransform.sizeDelta = value;
        }
    }

    public virtual void SetAlpha(float alpha) => fade.SetAlpha(alpha);

    protected virtual void Awake()
    {
        fadeImage = fadeImage ?? GetComponent<Image>();
        fade = new FadeTweenImage(fadeImage, 1f, true);
        SetAlpha(0f);
    }

    public virtual Tween FadeIn(float duration = 1f, float delay = 0f)
    {
        // Fade out black image to display screen
        return fade.Out(duration, delay).SetEase(Ease.InQuad);
    }

    public virtual Tween FadeOut(float duration = 1f, float delay = 0f)
    {
        // Fade in black image to hide screen
        return fade.In(duration, delay).SetEase(Ease.OutQuad);
    }
}
