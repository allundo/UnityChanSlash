using UnityEngine;
using System;
using DG.Tweening;
using UniRx;
using TMPro;

public class TextHandler : MonoBehaviour
{
    [SerializeField] private CharacterUI characterUI = default;
    [SerializeField] private ImageIcon imageIcon = default;
    [SerializeField] private TextMeshProUGUI tmTitle = default;

    private RectTransform rectTransform;
    private RectTransform iconRT;
    private Vector2 defaultPos;
    private Vector2 defaultSize;
    private Vector2 titleSize;
    private string currentSentence = "";
    private int currentLength = 0;
    private float currentLiterals = 0;
    private TextMeshProUGUI tm;
    private MessageData messageData;
    private int length;
    private int currentIndex = 0;
    private bool IsDisplayedAll => (int)currentLiterals == currentLength;
    private bool IsMessageEmpty => currentSentence == "";

    private Tween literalsTween = null;

    private bool isTapped = false;

    private ISubject<Unit> endOfSentence = new Subject<Unit>();
    public IObservable<Unit> EndOfSentence => endOfSentence;

    void Awake()
    {
        tm = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        defaultPos = rectTransform.anchoredPosition;
        defaultSize = rectTransform.sizeDelta;
        titleSize = tmTitle.GetComponent<RectTransform>().sizeDelta;
    }

    void Update()
    {
        isTapped = false;

        if (IsMessageEmpty) return;

        tm.text = currentSentence.Substring(0, (int)currentLiterals);
    }

    public void TapNext()
    {
        if (isTapped) return;
        isTapped = true;

        if (IsMessageEmpty) return;

        if (IsDisplayedAll)
        {
            SetNextSentence(++currentIndex);
        }
        else
        {
            literalsTween?.Complete();
        }
    }

    public void InputMessageData(MessageData data)
    {
        messageData = data;
        length = data.Source.Length;
        currentIndex = 0;
        SetNextSentence(currentIndex);
    }

    private void SetNextSentence(int index)
    {
        currentLiterals = 0f;

        if (index >= length)
        {
            tm.text = currentSentence = "";
            characterUI.DispFace(FaceID.NONE);
            SetImage(null, null);
            SetTitle(null);
            ResetTransform(Vector2.zero);

            endOfSentence.OnNext(Unit.Default);
            messageData.Read();
            return;
        }

        var currentData = messageData.Source[index];

        if (currentData.ignoreIfRead && messageData.isRead)
        {
            SetNextSentence(++currentIndex);
            return;
        }

        tm.fontSize = currentData.fontSize;
        tm.alignment = currentData.alignment;

        currentSentence = currentData.sentence;
        currentLength = currentSentence.Length;
        characterUI.DispFace(currentData.face);

        var deltaX = SetImage(currentData.spriteImage, currentData.matImage, currentData.caption);
        var deltaY = SetTitle(currentData.title);
        ResetTransform(new Vector2(deltaX, deltaY));

        if (currentData.literalsPerSec > 999.9f)
        {
            currentLiterals = currentLength;
            return;
        }

        literalsTween =
            DOTween
                .To(
                    () => currentLiterals,
                    value => currentLiterals = value,
                    currentLength,
                    currentLength / currentData.literalsPerSec
                )
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .Play();
    }

    private float SetImage(Sprite sprite, Material material, string caption = null)
    {
        imageIcon.SetCaption(caption);

        if (sprite == null && material == null)
        {
            imageIcon.SetEnabled(false);
            return 0f;
        }

        imageIcon.SetSource(sprite, material);
        imageIcon.SetEnabled(true);
        return imageIcon.ImageSpace * 0.5f;
    }

    private float SetTitle(string title = null)
    {
        if (title == null || title == "")
        {
            tmTitle.enabled = false;
            return 0f;
        }

        tmTitle.text = title;
        tmTitle.enabled = true;
        return -titleSize.y * 0.4f;
    }

    private void ResetTransform(Vector2 delta)
    {
        rectTransform.anchoredPosition = defaultPos + delta;
        rectTransform.sizeDelta = defaultSize - new Vector2(Mathf.Abs(delta.x), Mathf.Abs(delta.y)) * 2f;
    }
}
