using UnityEngine;

public interface ITargetPointer : ITargetingUI
{
    void SetVerticesPos(Vector2 pointerVec);
}

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(TargetPointerRenderer))]
public class TargetPointer : TargetingUI, ITargetPointer
{
    private TargetPointerRenderer pointerRenderer;

    protected override Color color
    {
        get { return pointerRenderer.GetVerticesColor(); }
        set { pointerRenderer.SetVerticesColor(value); }
    }

    protected override void Awake()
    {
        pointerRenderer = GetComponent<TargetPointerRenderer>();
        image = pointerRenderer;

        rectTransform = GetComponent<RectTransform>();

        targetColor = DefaultColor();
        Enabled = isActive = isChangingColor = false;
    }

    public void SetVerticesPos(Vector2 pointerVec) => pointerRenderer.SetVerticesPos(pointerVec);
}