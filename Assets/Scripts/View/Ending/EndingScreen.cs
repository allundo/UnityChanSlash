using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class EndingScreen : ScrollTextGenerator, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Sprite[] bgSprites = default;
    [SerializeField] private Color[] textColors = default;
    public EndingMessagesSource msgSource { get; private set; }

    private Vector2 startPos;
    private float moveY;

    private Image backGround;

    protected override void Awake()
    {
        base.Awake();

        backGround = GetComponent<Image>();
        backGround.raycastTarget = true;

    }

    public Sequence TextScrollSequence(int periodIndex = 0, float scrollSpeed = 1f, float intervalRate = 0.2f)
    {
        backGround.sprite = bgSprites[periodIndex];
        msgSource = Resources.Load<EndingMessagesData>("DataAssets/Message/EndingMessagesData").Param(periodIndex);

        startPos = new Vector2(0f, -Screen.height * 0.2f);
        moveY = Screen.height * 0.5f;

        float scrollDuration = 20f / scrollSpeed;
        float intervalTime = scrollDuration * intervalRate;

        var seq = DOTween.Sequence();

        foreach (var text in msgSource.Messages)
        {
            seq.AppendCallback(() => GenerateText(text, textColors[periodIndex], scrollDuration))
                .AppendInterval(intervalTime);
        }

        return seq.AppendInterval(scrollDuration)
            .AppendCallback(() => backGround.raycastTarget = false);
    }

    private void GenerateText(string text, Color fontColor, float duration, float fadeDuration = 2f)
    {
        Spawn(startPos, Mathf.Min(fadeDuration, duration * 0.5f), text, fontColor).ScrollY(moveY, duration).Play();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Time.timeScale = 6f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Time.timeScale = 1f;
    }
}
