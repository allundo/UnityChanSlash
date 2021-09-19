using UnityEngine;
using UnityEngine.UI;
public class FadeTweenImage : FadeTween<Image>
{
    public FadeTweenImage(Image image, float maxAlpha = 1f, bool isValidOnPause = false)
        : base(image, maxAlpha, isValidOnPause) { }
    public FadeTweenImage(GameObject gameObject, float maxAlpha = 1f, bool isValidOnPause = false)
        : base(gameObject.GetComponent<Image>(), maxAlpha, isValidOnPause) { }
    public void SetSprite(Sprite sprite)
    {
        image.sprite = sprite;
    }
}
