using UnityEngine;
using DG.Tweening;
using UniRx;
using TMPro;

public class TextHandler : MonoBehaviour
{
    [SerializeField] private float literalsPerSec = 20.0f;
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

    public ISubject<FaceID> subject = new Subject<FaceID>();

    void Awake()
    {
        tm = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {

        if (IsMessageEmpty) return;

        tm.text = currentSentence.Substring(0, (int)currentLiterals);
    }

    public void TapNext()
    {
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
        length = data.sentences.Length;
        currentIndex = 0;
        SetNextSentence(0);
    }

    private void SetNextSentence(int index)
    {
        currentLiterals = 0f;

        if (index >= length)
        {
            tm.text = currentSentence = "";
            subject.OnCompleted();
            return;
        }

        currentSentence = messageData.sentences[index];
        currentLength = currentSentence.Length;
        subject.OnNext(messageData.faces[index]);

        literalsTween =
            DOTween
                .To(
                    () => currentLiterals,
                    value => currentLiterals = value,
                    currentLength,
                    currentLength / literalsPerSec
                )
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .Play();
    }
}
