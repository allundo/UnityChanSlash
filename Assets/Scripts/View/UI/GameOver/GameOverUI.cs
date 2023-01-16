using UnityEngine.EventSystems;
using UnityEngine;
using DG.Tweening;

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
        seqEx.Skip();
    }

    private UITween uiTween;

    private SequenceEx seqEx = null;

    protected override void Awake()
    {
        fade = new FadeTween(gameObject, 0.25f, true);
        record.gameObject.SetActive(false);
        message.gameObject.SetActive(false);
        Inactivate();
    }

    protected virtual void Start()
    {
        uiTween = new UITween(gameOverBG.gameObject, true);

        seqEx = new SequenceEx()
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

        record.gameObject.SetActive(true);
        record.SetValues(rank, deadRecord);
        record.ResetPosition(new Vector2(0, -320f));
        record.SetRankEnable(false);

        // SequenceEx cannot handle a lot of callbacks so mediate it by original Sequence.
        // Maybe the implementation with deep nested callback cause stack overflow.
        var seq = DOTween.Sequence()
            .Append(record.SlideInTween())
            .Append(record.RankEffect(rank))
            .Append(record.RankPunchEffect(rank));

        if (rank > 0)
        {
            message.gameObject.SetActive(true);
            seq.Append(message.RankInTween());
        }

        seqEx.Append(seq).Play();
    }
}
