using UnityEngine;
using System;
using DG.Tweening;
using UniRx;
using TMPro;

public class TextHandler : MonoBehaviour
{
    [SerializeField] private CharacterUI characterUI = default;

    private RectTransform rectTransform;
    private Vector2 defaultPos;
    private Vector2 defaultSize;
    private string currentSentence = "";
    private int currentLength = 0;
    private float currentLiterals = 0;
    private TextMeshProUGUI tm;
    private MessageData[] messageData;
    private int length;
    private int currentIndex = 0;
    private bool IsDisplayedAll => (int)currentLiterals == currentLength;
    private bool IsMessageEmpty => currentSentence == "";

    private Tween literalsTween = null;

    private bool isTapped = false;

    private ISubject<Unit> sentence = new Subject<Unit>();
    public IObservable<Unit> Sentence => sentence.IgnoreElements();

    void Awake()
    {
        tm = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        defaultPos = rectTransform.anchoredPosition;
        defaultSize = rectTransform.sizeDelta;
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

    public void InputMessageData(MessageData[] data)
    {
        messageData = data;
        length = data.Length;
        currentIndex = 0;
        SetNextSentence(0);
    }

    private void SetNextSentence(int index)
    {
        currentLiterals = 0f;

        if (index >= length)
        {
            tm.text = currentSentence = "";
            characterUI.DispFace(FaceID.NONE);

            sentence.OnCompleted();
            // Initialize a Subject again because RepeatUntilDestroy() operator sends OnCompleted message ONLY ONCE when destroyed.
            sentence = new Subject<Unit>();
            return;
        }

        var currentData = messageData[index];

        tm.fontSize = currentData.fontSize;
        tm.alignment = currentData.alignment;

        currentSentence = currentData.sentence;
        currentLength = currentSentence.Length;
        characterUI.DispFace(currentData.face);

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

    public void MoveX(float moveX)
    {
        rectTransform.anchoredPosition += new Vector2(moveX, 0f);
        float moveDelta = rectTransform.anchoredPosition.x - defaultPos.x;
        rectTransform.sizeDelta = defaultSize - new Vector2(Mathf.Abs(moveDelta) * 2f, 0);
    }

    public void ResetTransform()
    {
        rectTransform.anchoredPosition = defaultPos;
        rectTransform.sizeDelta = defaultSize;

    }
}
