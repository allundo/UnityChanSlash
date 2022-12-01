using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

public abstract class BaseRecord : MonoBehaviour
{
    protected List<TextMeshProUGUI> textObjects = new List<TextMeshProUGUI>();
    protected List<Func<object, string>> textFormats = new List<Func<object, string>>();
    public RectTransform rectTransform { get; protected set; }

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        SetFormats();
        ResetPosition();
        SetActive(false);
    }

    protected abstract void SetFormats();

    public void SetValues(params object[] values)
    {
        SetActive(true);
        for (int i = 0; i < values.Length; i++)
        {
            textObjects[i].text = textFormats[i](values[i]);
        }
    }

    protected virtual void SetActive(bool isActive)
    {
        foreach (var obj in textObjects) obj.gameObject.SetActive(isActive);
    }

    public void ResetPosition() => ResetPosition(Vector2.zero);
    public void ResetPosition(Vector2 offset)
    {
        rectTransform.anchoredPosition = new Vector2(Screen.width, 0f) + offset;
    }

    public Tween SlideInTween(float duration = 0.5f)
    {
        return rectTransform.DOAnchorPosX(-Screen.width, duration).SetEase(Ease.OutQuart).SetRelative();
    }
}
