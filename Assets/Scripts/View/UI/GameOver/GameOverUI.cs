using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine;

public class GameOverUI : FadeActivate, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private FadeActivate gameOverTxt = default;
    [SerializeField] private FadeActivate gameOverBG = default;
    [SerializeField] private GameOverWindowUI selectUI = default;

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isActive) return;
        sequence.Skip();
    }

    private UITween uiTween;

    private SequenceEx sequence = null;

    protected override void Awake()
    {
        fade = new FadeTween(gameObject, 0.25f, true);
    }

    protected override void Start()
    {
        uiTween = new UITween(gameOverBG.gameObject, true);

        sequence = new SequenceEx()
            .Join(gameOverBG.FadeIn(2f))
            .Join(gameOverTxt.FadeIn(2f))
            .SetSkipable(false)
            .Append(FadeIn())
            .Join(uiTween.Move(new Vector2(0f, 380f), 1f))
            .Join(selectUI.FadeIn())
            .AppendCallback(selectUI.ActivateButtons);

        Inactivate();
    }

    public void Play()
    {
        gameObject.SetActive(true);
        sequence.Play();
    }
}