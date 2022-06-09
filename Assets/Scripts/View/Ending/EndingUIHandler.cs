using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UniRx;
using System;
using DG.Tweening;

public class EndingUIHandler : UISymbolGenerator, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private FadeScreen fade = default;
    [SerializeField] private Sprite[] bgSprites = default;
    private Vector2 startPos;
    private float moveY;

    private Image backGround;

    protected override void Awake()
    {
        base.Awake();

        backGround = GetComponent<Image>();
        backGround.raycastTarget = true;
    }

    public IObservable<Unit> StartScroll(int periodIndex = 0, float scrollSpeed = 1f, float intervalRate = 0.1f)
    {
        backGround.sprite = bgSprites[periodIndex];
        fade.color = Color.white;
        startPos = new Vector2(0f, -Screen.height * 0.2f);
        moveY = Screen.height * 0.5f;

        float scrollDuration = 10f / scrollSpeed;
        float intervalTime = scrollDuration * intervalRate;

        var seq = DOTween.Sequence()
            .Append(fade.FadeIn(3f, 0, false))
            .AppendCallback(() => fade.color = new Color(0, 0, 0, 0));

        foreach (var text in new string[] { "TEST" })
        {
            seq.AppendCallback(() => GenerateText(text, scrollDuration))
                .AppendInterval(intervalTime);
        }

        seq.AppendInterval(scrollDuration)
            .AppendCallback(() => fade.FadeOut(3f, 0, false).Play())
            .AppendInterval(3f)
            .AppendCallback(() => backGround.raycastTarget = false);

        return seq.SetUpdate(false).OnCompleteAsObservable(Unit.Default);
    }

    private void GenerateText(string text, float duration, float fadeDuration = 2f)
    {
        (Spawn(startPos, Mathf.Min(fadeDuration, duration * 0.5f)) as ScrollText)
            .SetText(text).ScrollY(moveY, duration).Play();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Time.timeScale = 3f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Time.timeScale = 1f;
    }
}
