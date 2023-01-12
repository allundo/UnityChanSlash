using UnityEngine;
using DG.Tweening;

public class MiniMapBase : MonoBehaviour
{
    protected RectTransform rectTransform;

    protected float landscapeSize = 420f;
    protected float portraitSize = 480f;
    protected float expandSize = 960f;
    protected float currentSize;
    protected Vector2 CurrentSize => new Vector2(currentSize, currentSize);
    protected Vector2 currentPos;
    protected Vector2 anchoredCenter;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        currentSize = rectTransform.sizeDelta.x;
        currentPos = anchoredCenter = rectTransform.anchoredPosition;
    }

    public virtual void InitUISize(float landscapeSize, float portraitSize, float expandSize)
    {
        this.landscapeSize = landscapeSize;
        this.portraitSize = portraitSize;
        this.expandSize = expandSize;
    }

    protected void SetSize(float size)
    {
        currentSize = size;
        rectTransform.sizeDelta = new Vector2(size, size);
    }
    protected void SetPortraitPos()
    {
        rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(1f, 0.5f);
        SetPos(new Vector2(-(portraitSize + 40f), portraitSize + 360f) * 0.5f);
        anchoredCenter = new Vector2(-Screen.width * 0.5f, 0f);
    }

    protected void SetLandscapePos()
    {
        rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(1f, 1f);
        SetPos(new Vector2(-(landscapeSize + 240f + Screen.width * ThirdPersonCamera.Margin), -(landscapeSize + 40f)) * 0.5f);
        anchoredCenter = -new Vector2(Screen.width, Screen.height) * 0.5f;
    }

    protected void SetPos(Vector2 pos)
    {
        rectTransform.anchoredPosition = currentPos = pos;
    }

    public virtual void ResetOrientation(DeviceOrientation orientation)
    {
        switch (orientation)
        {
            case DeviceOrientation.Portrait:
                SetSize(portraitSize);
                SetPortraitPos();
                break;

            case DeviceOrientation.LandscapeRight:
                SetSize(landscapeSize);
                SetLandscapePos();
                break;
        }
    }

    protected Tween HideTween(float duration = 0.25f)
    {
        return rectTransform.DOAnchorPosX(540f, duration).SetRelative(true).SetEase(Ease.OutCubic);
    }

    protected Tween ShowTween(float duration = 0.25f)
    {
        return rectTransform.DOAnchorPosX(-540f, duration).SetRelative(true).SetEase(Ease.OutCubic);
    }
}