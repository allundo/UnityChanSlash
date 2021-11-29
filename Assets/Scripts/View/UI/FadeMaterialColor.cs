using UnityEngine;
using UnityEngine.UI;

public class FadeMaterialColor : FadeTween
{
    protected Material material;
    protected GameObject gameObject;

    public FadeMaterialColor(MaskableGraphic image, float maxAlpha = 1f, bool isValidOnPause = false)
        : base(image, maxAlpha, isValidOnPause)
    {
        material = image.material;
        gameObject = image.gameObject;
    }

    public FadeMaterialColor(GameObject gameObject, float maxAlpha = 1f, bool isValidOnPause = false)
        : base(gameObject.GetComponent<MaskableGraphic>(), maxAlpha, isValidOnPause)
    {
        material = gameObject.GetComponent<Renderer>()?.material
            ?? gameObject.GetComponent<MaskableGraphic>().material;

        this.gameObject = gameObject;
    }

    public override Color color
    {
        get
        {
            return material.color;
        }
        set
        {
            material.color = value;
        }
    }

    public override void SetActive(bool isActive = true)
    {
        gameObject.SetActive(isActive);
    }
}
