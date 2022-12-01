using UnityEngine;
using TMPro;
using DG.Tweening;

public class TextTween : UITween
{
    protected TextMeshProUGUI txtMP;

    public TextTween(GameObject gameObject, bool isValidOnPause = false)
        : this(gameObject.GetComponent<TextMeshProUGUI>(), isValidOnPause) { }

    public TextTween(TextMeshProUGUI txtMP, bool isValidOnPause = false) : base(txtMP.gameObject, isValidOnPause)
    {
        this.txtMP = txtMP;
    }

    public override void SetSize(Vector2 size, bool setDefault = false)
    {
        rectTransform.sizeDelta = size;
        txtMP.fontSize = size.y;
        txtMP.enableWordWrapping = false;
        if (setDefault) defaultSize = size;
    }

    public override void ResetSize(float ratio = 1f)
    {
        rectTransform.sizeDelta = defaultSize * ratio;
        txtMP.fontSize = rectTransform.sizeDelta.y;
    }

    public override void ResetSize(float ratioX, float ratioY)
    {
        rectTransform.sizeDelta = new Vector2(defaultSize.x * ratioX, defaultSize.y * ratioY);
        txtMP.fontSize = rectTransform.sizeDelta.y * Mathf.Max(ratioX, ratioX / ratioY);
    }

    public override void ResetSizeY(float ratio = 1f)
        => ResetSize(rectTransform.sizeDelta.x / defaultSize.x, ratio);

    public override Tween Resize(Vector2 ratio, float duration = 1f, bool isReusable = false)
    {
        Tween resize = DOTween.Sequence()
                .Join(
                    rectTransform.DOSizeDelta(
                        new Vector2(defaultSize.x * ratio.x, defaultSize.y * ratio.y),
                        duration
                    )
                )
                .Join(ResizeFont(ratio, duration))
                .SetUpdate(isValidOnPause);

        return isReusable ? resize.AsReusable(gameObject) : resize;
    }

    protected Tween ResizeFont(Vector2 ratio, float duration = 1f)
    {
        return DOTween.To(
            () => txtMP.fontSize,
            size => txtMP.fontSize = size,
            defaultSize.y * ratio.y,
            duration
        );
    }
}