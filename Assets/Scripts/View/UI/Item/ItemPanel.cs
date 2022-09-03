using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;
using System;
using TMPro;

public class ItemPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI tmpNumOfItem = default;

    private RectTransform rectTransform;
    public int index { get; private set; }

    private RectTransform rtNumOfItem;

    private readonly Vector2 expand = new Vector2(140f, 140f);
    private readonly Vector2 shrink = new Vector2(100f, 100f);

    private ISubject<int> onPress = new Subject<int>();
    public IObservable<int> OnPress => onPress;

    private ISubject<int> onRelease = new Subject<int>();
    public IObservable<int> OnRelease => onRelease;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rtNumOfItem = tmpNumOfItem.GetComponent<RectTransform>();
        SetItemNum(0);
        ShrinkNum();
        rtNumOfItem.SetParent(transform.parent);
    }

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

    public void ExpandNum(Transform inventoryTf)
    {
        tmpNumOfItem.fontSize = 54f;
        rtNumOfItem.sizeDelta = expand;
        rtNumOfItem.SetAsLastSibling();
    }

    public void ShrinkNum()
    {
        tmpNumOfItem.fontSize = 36f;
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