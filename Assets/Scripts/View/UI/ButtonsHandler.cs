using System;
using System.Linq;
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

        Array.ForEach(buttons, btn =>
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
        Array.ForEach(buttons, btn => btn.SetInteractable());

        buttons[0].Select(true);
    }

    public void DisableInteraction(TwoPushButton exceptFor = null)
    {
        buttons
            .Where(btn => btn != exceptFor)
            .ToList()
            .ForEach(btn => btn.SetInteractable(false));
    }

    public void Activate(bool isInteractable = false)
    {
        Array.ForEach(buttons, btn => btn.gameObject.SetActive(true));
        selectIcon.gameObject.SetActive(true);
        if (isInteractable) EnableInteraction();
    }

    public void Inactivate()
    {
        Array.ForEach(buttons, btn => btn.gameObject.SetActive(false));
        selectIcon.gameObject.SetActive(false);
    }
}