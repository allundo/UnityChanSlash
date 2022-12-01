using UnityEngine.EventSystems;
using UnityEngine;

public class GameOverUI : FadeEnable, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private FadeEnable gameOverTxt = default;
    [SerializeField] private FadeEnable gameOverBG = default;
    [SerializeField] private GameOverWindowUI selectUI = default;
    [SerializeField] private DeadRecord record = default;

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
        record.gameObject.SetActive(false);
        fade = new FadeTween(gameObject, 0.25f, true);

        Inactivate();
    }

    protected virtual void Start()
    {
        uiTween = new UITween(gameOverBG.gameObject, true);

        sequence = new SequenceEx()
            .Join(gameOverBG.FadeIn(2f))
            .Join(gameOverTxt.FadeIn(2f))
            .SetSkippable(false)
            .Append(FadeIn())
            .Join(uiTween.Move(new Vector2(0f, 380f), 1f))
            .Join(selectUI.FadeIn())
            .AppendCallback(selectUI.ActivateButtons);
    }

    public void Play(int rank, DataStoreAgent.DeadRecord deadRecord)
    {
        record.ResetPosition(new Vector2(0, -320f));
        gameObject.SetActive(true);

        record.SetValues(rank, deadRecord);
        sequence.Append(record.SlideInTween());
        record.gameObject.SetActive(true);

        sequence.Play();
    }
}
