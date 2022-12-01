using UnityEngine.EventSystems;
using UnityEngine;

public class GameOverUI : FadeEnable, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private FadeEnable gameOverTxt = default;
    [SerializeField] private FadeEnable gameOverBG = default;
    [SerializeField] private GameOverWindowUI selectUI = default;
    [SerializeField] private DeadRecord record = default;
    [SerializeField] private RankInMessage message = default;

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
        gameObject.SetActive(true);

        record.SetValues(rank, deadRecord);
        record.ResetPosition(new Vector2(0, -320f));
        record.SetRankEnable(false);

        sequence
            .Append(record.SlideInTween())
            .Append(record.RankEffect(rank))
            .Append(record.RankPunchEffect(rank));

        if (rank > 0) sequence.Append(message.RankInTween());

        sequence.Play();
    }
}
