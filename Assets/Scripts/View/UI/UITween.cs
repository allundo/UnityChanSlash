using UnityEngine;
using DG.Tweening;

public class UITween
{
    protected GameObject gameObject;
    protected RectTransform rectTransform;
    public Vector2 defaultSize { get; protected set; }
    public Vector2 defaultPos { get; protected set; }
    protected bool isValidOnPause;

    public Vector2 CurrentPos => rectTransform.anchoredPosition;
    public Vector2 CurrentSize => rectTransform.sizeDelta;

    public UITween(GameObject gameObject, bool isValidOnPause = false)
    {
        this.gameObject = gameObject;
        rectTransform = gameObject.GetComponent<RectTransform>();
        defaultPos = CurrentPos;
        defaultSize = CurrentSize;

        this.isValidOnPause = isValidOnPause;
    }

    public Tween Move(Vector2 destPos, float duration = 1f, bool isReusable = false)
    {
        Tween move = rectTransform.DOAnchorPos(destPos, duration).SetUpdate(isValidOnPause);

        return isReusable ? move.AsReusable(gameObject) : move;
    }

    public Tween MoveRelative(Vector2 moveVec, float duration = 1f, bool isReusable = false)
    {
        return
            rectTransform
                .DOAnchorPos(CurrentPos + moveVec, duration)
                .SetRelative(true)
                .SetUpdate(isValidOnPause);
    }

    public Tween MoveBack(float duration = 1f, bool isReusable = false)
    {
        Tween move = rectTransform.DOAnchorPos(defaultPos, duration).SetUpdate(isValidOnPause);

        return isReusable ? move.AsReusable(gameObject) : move;
    }

    public void SetPos(Vector2 destPos)
    {
        rectTransform.anchoredPosition = destPos;
    }

    public void SetSize(Vector2 size)
    {
        rectTransform.sizeDelta = size;
    }

    public Tween Resize(float ratio = 1.5f, float duration = 1f, bool isReusable = false)
        => Resize(new Vector2(ratio, ratio), duration, isReusable);

    public Tween ResizeX(float ratioX = 1.5f, float duration = 1f, bool isReusable = false)
        => Resize(new Vector2(ratioX, 1f), duration, isReusable);

    public Tween ResizeY(float ratioY = 1.5f, float duration = 1f, bool isReusable = false)
        => Resize(new Vector2(1f, ratioY), duration, isReusable);

    public Tween Resize(Vector2 ratio, float duration = 1f, bool isReusable = false)
    {
        Tween resize =
            rectTransform
                .DOSizeDelta(new Vector2(defaultSize.x * ratio.x, defaultSize.y * ratio.y), duration)
                .SetUpdate(isValidOnPause);

        return isReusable ? resize.AsReusable(gameObject) : resize;
    }
}