using UnityEngine;
using UniRx;
using DG.Tweening;
using System.Linq;
using System;

public class SelectButtons : MonoBehaviour
{
    [SerializeField] public TwoPushButton startButton = default;
    [SerializeField] public TwoPushButton optionButton = default;
    [SerializeField] public TwoPushButton resultsButton = default;
    [SerializeField] private UnityChanIcon unityChanIcon = default;

    private UITween uiTween;

    public Vector2 Pos => uiTween.CurrentPos;

    private TwoPushButton currentButton;

    void Awake()
    {
        uiTween = new UITween(gameObject);
    }

    void Start()
    {
        startButton.Selected.Subscribe(button => SetCurrentButton(button)).AddTo(this);
        optionButton.Selected.Subscribe(button => SetCurrentButton(button)).AddTo(this);
        resultsButton.Selected.Subscribe(button => SetCurrentButton(button)).AddTo(this);

        startButton.OnClickAsObservable().Subscribe(button => InactivateButtons(button)).AddTo(this);
        optionButton.OnClickAsObservable().Subscribe(button => InactivateButtons(button)).AddTo(this);
        resultsButton.OnClickAsObservable().Subscribe(button => InactivateButtons(button)).AddTo(this);
    }

    private void SetCurrentButton(TwoPushButton button)
    {
        currentButton?.Deselect();
        currentButton = button;
        unityChanIcon.SelectTween(button.IconPos);
    }

    private void ActivateButtons()
    {
        startButton.SetInteractable();
        optionButton.SetInteractable();
        resultsButton.SetInteractable();

        startButton.Select(true);
    }

    private void InactivateButtons(TwoPushButton exceptFor = null)
    {
        new[] { startButton, optionButton, resultsButton }
            .Where(btn => btn != exceptFor)
            .ToList()
            .ForEach(btn => btn.SetInteractable(false));
    }

    public Tween TitleTween()
    {
        return
            DOTween.Sequence()
                .AppendCallback(() => uiTween.SetPosX(-2820f))
                .AppendInterval(0.4f)
                .Append(uiTween.MoveBack(0.6f).SetEase(Ease.Linear))
                .Append(Overrun(100f, 0.5f))
                .OnComplete(ActivateButtons);
    }

    private Tween Overrun(float overrun, float duration)
        => DOTween.Sequence()
            .Append(uiTween.MoveX(overrun, duration * 0.5f).SetEase(Ease.OutQuad))
            .Append(uiTween.MoveBack(duration * 0.5f).SetEase(Ease.InQuad));

    public Tween CameraOutTween() => uiTween.MoveY(1280f, 0.2f);
}
