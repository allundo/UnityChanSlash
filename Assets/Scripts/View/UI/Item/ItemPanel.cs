using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;
using System;
using TMPro;

public class ItemPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI tmpNumOfItem = default;
    [SerializeField] private float defaultSize = 100f;
    [SerializeField] private float fontSize = 36f;

    private RectTransform rectTransform;
    private int index;

    private RectTransform rtNumOfItem;

    private Vector2 expand;
    private Vector2 shrink;

    private ISubject<int> onPress = new Subject<int>();
    public IObservable<int> OnPress => onPress;

    private ISubject<int> onRelease = new Subject<int>();
    public IObservable<int> OnRelease => onRelease;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rtNumOfItem = tmpNumOfItem.GetComponent<RectTransform>();

        shrink = new Vector2(defaultSize, defaultSize);
        expand = shrink * 1.4f;
        SetItemNum(0);
        ShrinkNum();

        rtNumOfItem.SetParent(transform.parent);

    }

    public void SetEnabled(bool isEnable) => enabled = isEnable;

    /// <summary>
    /// Set number of items to display on Item inventory
    /// </summary>
    /// <param name="num">Number of items</param>
    public void SetItemNum(int num)
    {
        tmpNumOfItem.text = num > 0 ? num.ToString() : "";
        rtNumOfItem.SetAsLastSibling();
        rtNumOfItem.localPosition = transform.localPosition;
    }

    public void ExpandNum()
    {
        tmpNumOfItem.fontSize = fontSize * 1.5f;
        rtNumOfItem.sizeDelta = expand;
        rtNumOfItem.SetAsLastSibling();
    }

    public void ShrinkNum()
    {
        tmpNumOfItem.fontSize = fontSize;
        rtNumOfItem.sizeDelta = shrink;
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