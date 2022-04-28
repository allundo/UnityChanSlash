using UnityEngine;
using DG.Tweening;
using UniRx;
using TMPro;

public class TextHandler : MonoBehaviour
{
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

    private ISubject<FaceID> sentence = new Subject<FaceID>();
    public ISubject<FaceID> Sentence => sentence;

    void Awake()
    {
        tm = GetComponent<TextMeshProUGUI>();
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
            sentence.OnCompleted();
            // Initialize a Subject again because RepeatUntilDestroy() operator sends OnCompleted message ONLY ONCE when destroyed.
            sentence = new Subject<FaceID>();
            return;
        }

        var currentData = messageData[index];

        tm.fontSize = currentData.fontSize;
        tm.alignment = currentData.alignment;

        currentSentence = currentData.sentence;
        currentLength = currentSentence.Length;
        sentence.OnNext(currentData.face);

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
}
