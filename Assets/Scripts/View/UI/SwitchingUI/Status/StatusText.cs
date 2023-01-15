using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class StatusText : StatusContent
{
    private TextMeshProUGUI valueTMP = default;

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

    public override void SetSize(float ratio)
    {
        base.SetSize(ratio);
        valueTMP.fontSize = defaultValueFontSize * ratio;
    }
}
