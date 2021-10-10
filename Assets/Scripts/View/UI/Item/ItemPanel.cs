using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;
using System;

public class ItemPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rectTransform;
    public int index { get; private set; }

    private ISubject<int> onPress = new Subject<int>();
    public IObservable<int> OnPress => onPress;

    private ISubject<int> onRelease = new Subject<int>();
    public IObservable<int> OnRelease => onRelease;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public ItemPanel SetPos(Vector2 pos)
    {
        rectTransform.anchoredPosition = pos;
        return this;
    }

    public ItemPanel SetIndex(int index)
    {
        this.index = index;
        return this;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onPress.OnNext(index);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onRelease.OnNext(index);
    }
}