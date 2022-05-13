using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using UniRx;
using TMPro;

public class TextHandler : MonoBehaviour
{
    [SerializeField] private CharacterUI characterUI = default;
    [SerializeField] private Image icon = default;

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
        icon.enabled = false;
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
            DisableIcon();

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
        SetImage(currentData.spriteImage, currentData.matImage);

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

    private void DisableIcon() => SetImage(null, null);
    private void SetImage(Sprite sprite, Material material)
    {
        icon.sprite = sprite;
        icon.material = material;

        if (sprite == null && material == null)
        {
            icon.enabled = false;
            ResetTransform();
            return;
        }

        icon.enabled = true;
        SpaceLeft(icon.rectTransform.sizeDelta.x);
    }

    private void SpaceLeft(float sizeX) => SetPosX(sizeX * 0.5f);
    private void SpaceRight(float sizeX) => SetPosX(-sizeX * 0.5f);

    private void SetPosX(float deltaX)
    {
        rectTransform.anchoredPosition += new Vector2(deltaX, 0f);
        rectTransform.sizeDelta = defaultSize - new Vector2(Mathf.Abs(deltaX) * 2f, 0);
    }

    private void ResetTransform()
    {
        rectTransform.anchoredPosition = defaultPos;
        rectTransform.sizeDelta = defaultSize;
    }
}
