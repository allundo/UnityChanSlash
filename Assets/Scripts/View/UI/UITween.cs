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
    public Vector2 CurrentScreenPos => rectTransform.position;
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
    public Tween MoveOffset(Vector2 offset, float duration = 1f, bool isReusable = false)
        => Move(defaultPos + offset, duration, isReusable);

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
        => Move(defaultPos, duration, isReusable);

    /// <summary>
    /// Returns Tween of moving back vector to default position with distance ratio.
    /// </summary>
    /// <param name="ratio">Normalized destination between current to default position</param>
    public Tween MoveBackRatio(float duration = 1f, float ratio = 1f, bool isReusable = false)
        => Move((rectTransform.anchoredPosition - defaultPos) * (1f - ratio), duration, isReusable);

    public Tween Jump(float destX, float destY, float duration = 1f, float jumpPower = 1000f, int numJumps = 1)
        => rectTransform.DOJump(new Vector3(destX, destY), jumpPower, numJumps, duration);

    public Tween Jump(Vector2 dest, float duration = 1f, float jumpPower = 1000f, int numJumps = 1)
        => Jump(dest.x, dest.y, duration, jumpPower, numJumps);

    public Tween Rotate(Vector3 endValue, float duration = 1f, bool isBeyond360 = true)
        => rectTransform.DORotate(endValue, duration, isBeyond360 ? RotateMode.FastBeyond360 : RotateMode.Fast);

    public Tween Rotate(float endValue, float duration = 1f, bool isBeyond360 = true)
        => Rotate(new Vector3(0f, 0f, endValue), duration, isBeyond360);

    public Tween Punch(Vector2 punchVec, float duration = 1f, int vibrato = 10, float elasticity = 1)
        => rectTransform.DOPunchAnchorPos(punchVec, duration, vibrato, elasticity);

    public Tween PunchY(float strength, float duration = 1f, int vibrato = 10, float elasticity = 1)
        => rectTransform.DOPunchAnchorPos(new Vector2(0f, strength), duration, vibrato, elasticity);

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

    public void SetScreenPos(Vector2 destPos)
    {
        rectTransform.position = destPos;
    }

    public void SetPosX(float destX) => SetPos(new Vector2(destX, defaultPos.y));
    public void SetPosY(float destY) => SetPos(new Vector2(defaultPos.x, destY));

    /// <summary>
    /// Set image position by offset vector from default position
    /// </summary>
    /// <param name="offset">Offset vector from default position</param>
    public void SetPosOffset(Vector2 offset)
    {
        rectTransform.anchoredPosition = defaultPos + offset;
    }

    public void ResetPos()
    {
        rectTransform.anchoredPosition = defaultPos;
    }

    public virtual void SetSize(Vector2 size, bool setDefault = false)
    {
        rectTransform.sizeDelta = size;
        if (setDefault) defaultSize = size;
    }

    public virtual void ResetSize(float ratio = 1f)
    {
        rectTransform.sizeDelta = defaultSize * ratio;
    }

    public virtual void ResetSize(float ratioX, float ratioY)
    {
        rectTransform.sizeDelta = new Vector2(defaultSize.x * ratioX, defaultSize.y * ratioY);
    }

    public void ResetSizeX(float ratio = 1f)
    {
        rectTransform.sizeDelta = new Vector2(defaultSize.x * ratio, rectTransform.sizeDelta.y);
    }

    public virtual void ResetSizeY(float ratio = 1f)
    {
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, defaultSize.y * ratio);
    }

    public Tween Resize(float ratio = 1.5f, float duration = 1f, bool isReusable = false)
        => Resize(new Vector2(ratio, ratio), duration, isReusable);

    public Tween ResizeX(float ratioX = 1.5f, float duration = 1f, bool isReusable = false)
        => Resize(new Vector2(ratioX, 1f), duration, isReusable);

    public Tween ResizeY(float ratioY = 1.5f, float duration = 1f, bool isReusable = false)
        => Resize(new Vector2(1f, ratioY), duration, isReusable);

    public virtual Tween Resize(Vector2 ratio, float duration = 1f, bool isReusable = false)
    {
        Tween resize =
            rectTransform
                .DOSizeDelta(new Vector2(defaultSize.x * ratio.x, defaultSize.y * ratio.y), duration)
                .SetUpdate(isValidOnPause);

        return isReusable ? resize.AsReusable(gameObject) : resize;
    }
}