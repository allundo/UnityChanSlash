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

    void Start()
    {
        fade.color = Color.white;
        startPos = new Vector2(0f, -Screen.height * 0.2f);
        moveY = Screen.height * 0.5f;
    }

    public IObservable<Unit> StartScroll(int periodIndex = 0)
    {
        backGround.sprite = bgSprites[periodIndex];

        var seq = DOTween.Sequence()
            .Append(fade.FadeIn(3f, 0, false))
            .AppendCallback(() => fade.color = new Color(0, 0, 0, 0));

        seq.Append(GenerateText("TEST"));

        seq.AppendInterval(1f)
            .AppendCallback(() => fade.FadeOut(3f, 0, false).Play())
            .AppendInterval(3f)
            .AppendCallback(() => backGround.raycastTarget = false);

        return seq.SetUpdate(false).OnCompleteAsObservable(Unit.Default);
    }

    private Tween GenerateText(string text)
    {
        return (Spawn(startPos, 2f) as ScrollText).SetText(text).ScrollY(moveY);
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
