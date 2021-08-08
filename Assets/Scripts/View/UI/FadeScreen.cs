using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeScreen : MonoBehaviour
{
    private FadeTween fade;
    void Awake()
    {
        fade = new FadeTween(gameObject, 1f, true);
    }

    public Tween FadeIn(float duration = 1f, float delay = 0f)
    {
        // Fade out black image to display screen
        return fade.Out(duration, delay);
    }

    public Tween FadeOut(float duration = 1f, float delay = 0f)
    {
        // Fade in black image to hide screen
        return fade.In(duration, delay);
    }
}
