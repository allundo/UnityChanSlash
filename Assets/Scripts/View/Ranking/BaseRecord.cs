using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;

public abstract class BaseRecord : MonoBehaviour
{
    protected List<TextMeshProUGUI> textObjects = new List<TextMeshProUGUI>();
    protected List<Func<object, string>> textFormats = new List<Func<object, string>>();
    public RectTransform rectTransform { get; protected set; }

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        SetFormats();
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
}