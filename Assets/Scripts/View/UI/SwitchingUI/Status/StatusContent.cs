using UnityEngine;
using UnityEngine.UI;
using TMPro;

public interface IStatusContent
{
    void SetValue(float value);
    void SetSubValues(params float[] values);
    void SetSize(float ratio);
    void Expand();
    void Shrink();
}

public abstract class StatusContent : MonoBehaviour, IStatusContent
{
    [SerializeField] protected TextMeshProUGUI label = default;
    [SerializeField] protected StatusContent[] subStatus = default;
    [SerializeField] protected string expandLabel = "";
    [SerializeField] protected Vector2 expandOffsetPos = default;
    [SerializeField] protected float expandRatio = 1f;
    [SerializeField] protected float labelExpandOffset = 0f;

    protected MaskableGraphic value = default;

    private RectTransform labelRT;
    private RectTransform valueRT;

    private Vector2 defaultLabelPos;
    private Vector2 defaultLabelSize;
    private string defaultLabelText;
    private float defaultLabelFontSize;
    private float defaultLabelAlpha;

    private Vector2 defaultValuePos;
    private Vector2 defaultValueSize;

    private float currentRatio = 1f;

    protected virtual void Awake()
    {
        value = GetComponent<MaskableGraphic>();
        labelRT = label.GetComponent<RectTransform>();
        valueRT = value.GetComponent<RectTransform>();

        defaultLabelPos = labelRT.anchoredPosition;
        defaultLabelSize = labelRT.sizeDelta;
        defaultLabelText = label.text;
        defaultLabelFontSize = label.fontSize;
        defaultLabelAlpha = label.color.a;

        defaultValuePos = valueRT.anchoredPosition;
        defaultValueSize = valueRT.sizeDelta;

        currentRatio = 1f;
    }

    public abstract void SetValue(float value);

    public void SetSubValues(params float[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            subStatus[i].SetValue(values[i]);
        }
    }

    public void SetSize(float ratio)
    {
        SetSizeAndPos(ratio);
        subStatus.ForEach(status => status.SetSize(ratio));

        currentRatio = ratio;
    }

    protected void SetSizeAndPos(float ratio) => SetSizeAndPos(ratio, Vector2.zero);
    protected virtual void SetSizeAndPos(float ratio, Vector2 offsetPos)
    {
        valueRT.anchoredPosition = defaultValuePos * ratio + offsetPos;
        valueRT.sizeDelta = defaultValueSize * ratio;

        labelRT.anchoredPosition = defaultLabelPos * ratio;
        labelRT.sizeDelta = defaultLabelSize * ratio;
        label.fontSize = defaultLabelFontSize * ratio;
    }

    public void ResetSize() => SetSize(currentRatio);

    protected virtual void SetAlpha(float alpha)
    {
        label.alpha = alpha;
    }

    public void Expand()
    {
        SetSizeAndPos(expandRatio, expandOffsetPos);

        var labelPos = labelRT.anchoredPosition;
        labelRT.anchoredPosition = new Vector2(labelPos.x + labelExpandOffset, labelPos.y);

        if (expandLabel != "") label.text = expandLabel;

        SetOpaque();

        subStatus.ForEach(status => status.Expand());
    }

    public void Shrink()
    {
        ResetSize();
        label.text = defaultLabelText;
        SetTransparent();

        subStatus.ForEach(status => status.Shrink());
    }

    public virtual void SetTransparent()
    {
        SetAlpha(defaultLabelAlpha);
        subStatus.ForEach(status => status.SetTransparent());
    }

    public virtual void SetOpaque()
    {
        SetAlpha(1f);
        subStatus.ForEach(status => status.SetOpaque());
    }
}
