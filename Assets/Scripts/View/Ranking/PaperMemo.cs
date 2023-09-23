using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UniRx;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class PaperMemo : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private TextMeshProUGUI diaryNumber = default;
    [SerializeField] private Sprite closeImage = default;
    [SerializeField] private Sprite selectImage = default;
    [SerializeField] private Sprite openImage = default;

    private Image imgMemo;
    private RectTransform rtImg;
    private RectTransform rtText;
    private Tween textTween;
    private float defaultFontSize = 50;
    private Vector2 defaultTxtPos;

    public MessageData diaryData { get; private set; }

    private ISubject<Unit> pressSubject = new Subject<Unit>();
    public IObservable<Unit> Press => pressSubject;

    private ISubject<PaperMemo> selected = new Subject<PaperMemo>();
    public IObservable<PaperMemo> Selected => selected;

    private bool isSelected = false;
    private bool interactable = true;

    void Awake()
    {
        imgMemo = GetComponent<Image>();
        rtImg = imgMemo.GetComponent<RectTransform>();
        rtText = diaryNumber.GetComponent<RectTransform>();

        textTween = DOTween.Sequence()
            .Join(rtText.DOMoveY(60f, 0.2f).SetRelative().SetEase(Ease.OutCubic))
            .Join(DOVirtual.Float(defaultFontSize * 2f, defaultFontSize, 0.2f, value => diaryNumber.fontSize = value).SetEase(Ease.OutCubic))
            .AsReusable(gameObject);

        defaultFontSize = diaryNumber.fontSize;
        defaultTxtPos = rtText.anchoredPosition;
    }

    public PaperMemo SetID(int id)
    {
        diaryNumber.text = $"{id}";
        return this;
    }

    public PaperMemo SetPos(float x, float y)
    {
        rtImg.anchoredPosition = new Vector2(x, y);
        return this;
    }
    public PaperMemo SetMessage(MessageData data)
    {
        diaryData = data;
        return this;
    }

    public void SetInteractable(bool interactable)
    {
        this.interactable = interactable;
    }

    public void Select()
    {
        if (isSelected || !interactable) return;

        isSelected = true;
        imgMemo.sprite = selectImage;
        diaryNumber.fontSize = defaultFontSize * 2f;
        selected.OnNext(this);
    }

    public void Open()
    {
        imgMemo.sprite = openImage;
        textTween.Restart();
    }

    public void Close()
    {
        imgMemo.sprite = selectImage;
        textTween.PlayBackwards();
    }

    public void Deselect()
    {
        isSelected = false;
        imgMemo.sprite = closeImage;

        textTween.Rewind();
        rtText.anchoredPosition = defaultTxtPos;
        diaryNumber.fontSize = defaultFontSize;
    }

    public void OnPointerEnter(PointerEventData eventData) => Select();

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (interactable && isSelected) pressSubject.OnNext(Unit.Default);
    }
}
