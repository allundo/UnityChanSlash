using UnityEngine;
using UnityEngine.UI;
using TMPro;

public interface IStatusContent
{
    void SetValue(float value);
    void SetLabel(string label);
    void SetSubValues(params float[] values);
    void SetSize(float ratio);
}

public abstract class StatusContent : MonoBehaviour, IStatusContent
{
    [SerializeField] private TextMeshProUGUI label = default;
    [SerializeField] protected StatusContent[] subStatus = default;

    protected MaskableGraphic value = default;

    private RectTransform labelRT;
    private RectTransform valueRT;

    private Vector2 defaultLabelPos;
    private Vector2 defaultLabelSize;
    private string defaultLabelText;
    private float defaultLabelFontSize;

    private Vector2 defaultValuePos;
    private Vector2 defaultValueSize;

    protected virtual void Awake()
    {
        value = GetComponent<MaskableGraphic>();
        labelRT = label.GetComponent<RectTransform>();
        valueRT = value.GetComponent<RectTransform>();

        defaultLabelPos = labelRT.anchoredPosition;
        defaultLabelSize = labelRT.sizeDelta;
        defaultLabelText = label.text;
        defaultLabelFontSize = label.fontSize;

        defaultValuePos = valueRT.anchoredPosition;
        defaultValueSize = valueRT.sizeDelta;
    }

    public abstract void SetValue(float value);

    public void SetSubValues(params float[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            subStatus[i].SetValue(values[i]);
        }
    }

    public void SetLabel(string label)
    {
        this.label.text = label;
    }

    public void ResetLabel() => SetLabel(defaultLabelText);

    public virtual void SetSize(float ratio)
    {
        labelRT.anchoredPosition = defaultLabelPos * ratio;
        labelRT.sizeDelta = defaultLabelSize * ratio;
        label.fontSize = defaultLabelFontSize * ratio;

        valueRT.anchoredPosition = defaultValuePos * ratio;
        valueRT.sizeDelta = defaultValueSize * ratio;

        subStatus.ForEach(status => status.SetSize(ratio));
    }

    public void ResetSize() => SetSize(1f);
}
