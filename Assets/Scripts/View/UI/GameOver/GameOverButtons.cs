using UnityEngine;
using UniRx;

public class GameOverButtons : MonoBehaviour
{
    [SerializeField] public TwoPushButton restartButton = default;
    [SerializeField] public TwoPushButton titleButton = default;
    [SerializeField] private SelectIcon unityChanIcon = default;

    private TwoPushButton currentButton;

    void Start()
    {
        restartButton.Selected.Subscribe(button => SetCurrentButton(button)).AddTo(this);
        titleButton.Selected.Subscribe(button => SetCurrentButton(button)).AddTo(this);
    }

    private void SetCurrentButton(TwoPushButton button)
    {
        currentButton?.Deselect();
        currentButton = button;
        unityChanIcon.SelectTween(button.Pos);
    }

    private void ActivateButtons()
    {
        restartButton.SetInteractable();
        titleButton.SetInteractable();

        restartButton.Select(true);
    }
}
