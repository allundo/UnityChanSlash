using UnityEngine;
using UniRx;
using System.Linq;

public class GameOverWindowUI : MessageWindowUI
{
    [SerializeField] public TwoPushButton restartButton = default;
    [SerializeField] public TwoPushButton titleButton = default;
    [SerializeField] private SelectIcon unityChanIcon = default;

    private TwoPushButton currentButton;

    protected override void Start()
    {
        restartButton.Selected.Subscribe(button => SetCurrentButton(button)).AddTo(this);
        titleButton.Selected.Subscribe(button => SetCurrentButton(button)).AddTo(this);

        restartButton.OnClickAsObservable().Subscribe(button => InactivateButtons(button)).AddTo(this);
        titleButton.OnClickAsObservable().Subscribe(button => InactivateButtons(button)).AddTo(this);

        restartButton.gameObject.SetActive(false);
        titleButton.gameObject.SetActive(false);
        unityChanIcon.gameObject.SetActive(false);

        Inactivate();
    }

    private void SetCurrentButton(TwoPushButton button)
    {
        currentButton?.Deselect();
        currentButton = button;
        unityChanIcon.SelectTween(button.IconPos);
    }

    public void ActivateButtons()
    {
        restartButton.gameObject.SetActive(true);
        titleButton.gameObject.SetActive(true);
        unityChanIcon.gameObject.SetActive(true);

        restartButton.SetInteractable();
        titleButton.SetInteractable();

        restartButton.Select(true);
    }

    private void InactivateButtons(TwoPushButton exceptFor = null)
    {
        new[] { restartButton, titleButton }
            .Where(btn => btn != exceptFor)
            .ToList()
            .ForEach(btn => btn.SetInteractable(false));
    }
}
