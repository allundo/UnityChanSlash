using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class StatusText : StatusContent
{
    protected TextMeshProUGUI valueTMP;

    private float defaultValueFontSize;

    protected override void Awake()
    {
        base.Awake();
        valueTMP = value as TextMeshProUGUI;
        defaultValueFontSize = valueTMP.fontSize;
    }

    protected void SetValue(int value)
    {
        this.valueTMP.text = value.ToString();
    }

    public override void SetValue(float value)
        => SetValue(Mathf.RoundToInt(value));

    public void SetValue(string value)
    {
        this.valueTMP.text = value;
    }

    protected override void SetSizeAndPos(float ratio, Vector2 contentPos)
    {
        base.SetSizeAndPos(ratio, contentPos);
        valueTMP.fontSize = defaultValueFontSize * ratio;
    }

    protected override void SetAlpha(float alpha)
    {
        base.SetAlpha(alpha);
        valueTMP.alpha = alpha;
    }
}
