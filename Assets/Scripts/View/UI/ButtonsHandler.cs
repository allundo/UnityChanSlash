using UniRx;
using UnityEngine;

public class ButtonsHandler
{
    private TwoPushButton[] buttons;
    private TwoPushButton currentButton;
    private SelectIcon selectIcon;
    private AudioSource selectSnd;
    private AudioSource decisionSnd;

    public ButtonsHandler(SelectIcon selectIcon, params TwoPushButton[] buttons)
    {
        this.buttons = buttons;
        this.selectIcon = selectIcon;

        var resource = ResourceLoader.Instance;
        selectSnd = resource.LoadSnd(SNDType.Select);
        decisionSnd = resource.LoadSnd(SNDType.Decision);

        buttons.ForEach(btn =>
        {
            btn.Selected.Subscribe(button =>
            {
                selectSnd.PlayEx();
                SetCurrentButton(button);
            })
            .AddTo(btn);

            btn.OnClickAsObservable().Subscribe(button =>
            {
                decisionSnd.PlayEx();
                DisableInteraction(button);
            })
            .AddTo(btn);
        });
    }

    private void SetCurrentButton(TwoPushButton button)
    {
        selectSnd.PlayEx();
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
