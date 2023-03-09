using UnityEngine;
using DG.Tweening;
using System.Linq;

public class SelectButtons : MonoBehaviour
{
    [SerializeField] public TwoPushButton startButton = default;
    [SerializeField] public TwoPushButton settingsButton = default;
    [SerializeField] public TwoPushButton resultsButton = default;
    [SerializeField] private UnityChanIcon unityChanIcon = default;

    private UITween uiTween;

    public Vector2 Pos => uiTween.CurrentPos;

    private ButtonsHandler handler;

    void Awake()
    {
        uiTween = new UITween(gameObject);
    }

    void Start()
    {
        handler = new ButtonsHandler(unityChanIcon, startButton, settingsButton, resultsButton);
    }

    public Tween TitleTween()
    {
        return
            DOTween.Sequence()
                .AppendCallback(() => uiTween.SetPosX(-2820f))
                .AppendInterval(0.4f)
                .Append(uiTween.MoveBack(0.6f).SetEase(Ease.Linear))
                .Append(Overrun(100f, 0.5f))
                .OnComplete(handler.EnableInteraction);
    }

    private Tween Overrun(float overrun, float duration)
        => DOTween.Sequence()
            .Append(uiTween.MoveX(overrun, duration * 0.5f).SetEase(Ease.OutQuad))
            .Append(uiTween.MoveBack(duration * 0.5f).SetEase(Ease.InQuad));

    public Tween CameraOutTween() => uiTween.MoveY(1920f, 0.2f);
}
