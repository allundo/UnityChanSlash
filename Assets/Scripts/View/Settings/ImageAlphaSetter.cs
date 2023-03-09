using UnityEngine;
using UnityEngine.UI;

public class ImageAlphaSetter : MonoBehaviour
{
    [SerializeField] protected float maxAlpha = 1f;

    protected MaskableGraphic image;

    public virtual Color color
    {
        get { return image.color; }
        set { image.color = value; }
    }

    public float alpha
    {
        get { return color.a / maxAlpha; }
        set { color = new(color.r, color.g, color.b, value * maxAlpha); }
    }

    public void SetDisplay(bool isEnable)
    {
        image.enabled = isEnable;
    }

    protected virtual void Awake()
    {
        image = GetComponent<MaskableGraphic>();
        alpha = 1f;
    }
}