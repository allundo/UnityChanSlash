using UnityEngine;
using UnityEngine.UI;

public abstract class FadeColor : MonoBehaviour
{
    /// <summary>
    /// Asymptotic speed of color cross fading
    /// </summary>
    [SerializeField] private float fadeRatio = 0.5f;

    protected MaskableGraphic image;
    protected RectTransform rectTransform;

    protected bool isActive;
    protected bool isChangingColor;
    protected Color targetColor;

    protected virtual Color color
    {
        get { return image.color; }
        set { image.color = value; }
    }
    protected abstract Color DefaultColor();
    protected abstract Color ChangedColor();

    protected virtual bool Enabled
    {
        get { return image.enabled; }
        set { image.enabled = value; }
    }

    protected virtual void Awake()
    {
        image = GetComponent<MaskableGraphic>();
        rectTransform = GetComponent<RectTransform>();

        targetColor = DefaultColor();
        Enabled = isActive = isChangingColor = false;
    }

    void Update()
    {
        if (!isChangingColor) return;

        var deltaColor = targetColor - color;

        Color newColor;
        if ((((Vector4)deltaColor).sqrMagnitude > 0.01f))
        {
            newColor = color + deltaColor * fadeRatio;
        }
        else
        {
            newColor = targetColor;
            isChangingColor = false;
        }

        color = newColor;

        if (deltaColor.a < 0f && color.a < 0.001f)
        {
            Enabled = isActive = false;
        }
    }

    public void Show(Vector2 pos)
    {
        OnShow(pos);

        if (isActive) return;

        OnActive(pos);

        var defaultColor = DefaultColor();
        color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, color.a);

        SetTargetColor(defaultColor);

        Enabled = isActive = true;
    }

    protected virtual void OnShow(Vector2 pos)
    {
        rectTransform.position = pos;
    }

    protected virtual void OnActive(Vector2 pos) { }

    public void Hide()
    {
        OnHide();

        if (!isActive) return;

        OnInactive();

        SetTargetColor(targetColor, false);
        isActive = false;
    }

    protected virtual void OnHide() { }
    protected virtual void OnInactive() { }

    public void SwitchColor(bool isChanged = false)
        => SetTargetColor(isChanged ? ChangedColor() : DefaultColor(), isActive);

    protected void SetTargetColor(Color color)
    {
        targetColor = color;
        isChangingColor = true;
    }

    protected void SetTargetColor(Color color, bool isActive)
        => SetTargetColor(isActive ? color : new Color(color.r, color.g, color.b, 0f));

    protected void SetTargetColor(Color color, float alpha)
        => SetTargetColor(new Color(color.r, color.g, color.b, alpha));
}
