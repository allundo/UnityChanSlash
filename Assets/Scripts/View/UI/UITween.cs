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

    public Tween MoveRelative(Vector2 moveVec, float duration = 1f)
    {
        return
            rectTransform
                .DOAnchorPos(moveVec, duration)
                .SetRelative(true)
                .SetUpdate(isValidOnPause);
    }

    public Tween MoveX(float moveX, float duration = 1f)
        => MoveRelative(new Vector2(moveX, 0f), duration);

    public Tween MoveY(float moveY, float duration = 1f)
        => MoveRelative(new Vector2(0f, moveY), duration);

    /// <summary>
    /// Returns Tween of moving back to default position.
    /// </summary>
    public Tween MoveBack(float duration = 1f, bool isReusable = false)
    {
        Tween move = rectTransform.DOAnchorPos(defaultPos, duration).SetUpdate(isValidOnPause);

        return isReusable ? move.AsReusable(gameObject) : move;
    }

    public Tween Jump(float destX, float destY, float duration = 1f, float jumpPower = 1000f, int numJumps = 1)
        => rectTransform.DOJump(new Vector3(destX, destY), jumpPower, numJumps, duration);

    public Tween Jump(Vector2 dest, float duration = 1f, float jumpPower = 1000f, int numJumps = 1)
        => Jump(dest.x, dest.y, duration, jumpPower, numJumps);

    public Tween Rotate(Vector3 endValue, float duration = 1f, bool isBeyond360 = true)
        => rectTransform.DORotate(endValue, duration, isBeyond360 ? RotateMode.FastBeyond360 : RotateMode.Fast);

    public Tween Rotate(float endValue, float duration = 1f, bool isBeyond360 = true)
        => Rotate(new Vector3(0f, 0f, endValue), duration, isBeyond360);

    /// <summary>
    /// Set position and move immediately.
    /// </summary>
    /// <param name="destPos"></param>
    /// <param name="setDefault">sets destPos as default position if true</param>
    public void SetPos(Vector2 destPos, bool setDefault = false)
    {
        rectTransform.anchoredPosition = destPos;
        if (setDefault) defaultPos = destPos;
    }

    public void SetPosX(float destX) => SetPos(new Vector2(destX, defaultPos.y));
    public void SetPos(float destY) => SetPos(new Vector2(defaultPos.x, destY));

    public void SetSize(Vector2 size, bool setDefault = false)
    {
        rectTransform.sizeDelta = size;
        if (setDefault) defaultSize = size;
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