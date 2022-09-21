using UniRx;

public class ButtonsHandler
{
    private TwoPushButton[] buttons;
    private TwoPushButton currentButton;
    private SelectIcon selectIcon;

    public ButtonsHandler(SelectIcon selectIcon, params TwoPushButton[] buttons)
    {
        this.buttons = buttons;
        this.selectIcon = selectIcon;

        buttons.ForEach(btn =>
        {
            btn.Selected.Subscribe(button => SetCurrentButton(button)).AddTo(btn);
            btn.OnClickAsObservable().Subscribe(button => DisableInteraction(button)).AddTo(btn);
        });
    }

    private void SetCurrentButton(TwoPushButton button)
    {
        currentButton?.Deselect();
        currentButton = button;
        selectIcon.SelectTween(button.IconPos);
    }

    public void EnableInteraction()
    {
        buttons.ForEach(btn => btn.SetInteractable());

        buttons[0].Select(true);
    }

    public void DisableInteraction(TwoPushButton exceptFor = null)
    {
        buttons.ForEach(btn => btn.SetInteractable(false), exceptFor);
    }

    public void Activate(bool isInteractable = false)
    {
        buttons.ForEach(btn => btn.gameObject.SetActive(true));
        selectIcon.gameObject.SetActive(true);
        if (isInteractable) EnableInteraction();
    }

    public void Inactivate()
    {
        buttons.ForEach(btn => btn.Inactivate());
        selectIcon.gameObject.SetActive(false);
    }
}
