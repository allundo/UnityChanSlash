using UnityEngine;
using DG.Tweening;

public class SwitchingUIBase : MonoBehaviour
{
    protected RectTransform rectTransform;

    protected float landscapeSize = 420f;
    protected float portraitSize = 480f;
    protected float expandSize = 960f;
    protected float currentSize;
    protected Vector2 CurrentSize => new Vector2(currentSize, currentSize);
    protected Vector2 currentPos;
    protected Vector2 anchoredCenter;
    protected bool isShown = true;

    protected Vector2 PortraitPos => new Vector2(-(portraitSize + 40f), portraitSize + 360f) * 0.5f;
    protected Vector2 LandscapePos => new Vector2(-(landscapeSize + 280f + Mathf.Max(120f, Screen.width * ThirdPersonCamera.Margin)), -(landscapeSize + 40f)) * 0.5f;

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
    protected virtual void SetPortraitPos()
    {
        rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(1f, 0.5f);
        SetPos(PortraitPos);
        anchoredCenter = new Vector2(-Screen.width * 0.5f, 0f);
    }

    protected virtual void SetLandscapePos()
    {
        rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(1f, 1f);
        SetPos(LandscapePos);
        anchoredCenter = -new Vector2(Screen.width, Screen.height) * 0.5f;
    }

    protected void SetPos(Vector2 pos)
    {
        rectTransform.anchoredPosition = currentPos = isShown ? pos : new Vector2(-pos.x, pos.y);
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

    protected Tween SwitchUI(float duration = 0.25f, TweenCallback onComplete = null)
    {
        onComplete = onComplete ?? (() => { });
        currentPos = new Vector2(-currentPos.x, currentPos.y);

        return rectTransform.DOAnchorPos(currentPos, duration)
            .OnPlay(() => isShown = !isShown)
            .OnComplete(onComplete)
            .SetEase(Ease.OutCubic)
            .Play();
    }
}
