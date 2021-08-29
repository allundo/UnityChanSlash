using UnityEngine;
using UnityEngine.UI;

public class FadeMaterialColor : FadeTween
{
    public FadeMaterialColor(MaskableGraphic image, float maxAlpha = 1f, bool isValidOnPause = false)
        : base(image, maxAlpha, isValidOnPause) { }

    public FadeMaterialColor(GameObject gameObject, float maxAlpha = 1f, bool isValidOnPause = false)
        : this(gameObject.GetComponent<MaskableGraphic>(), maxAlpha, isValidOnPause) { }

    protected override Color color
    {
        get
        {
            return image.material.color;
        }
        set
        {
            image.material.color = value;
        }
    }
}
