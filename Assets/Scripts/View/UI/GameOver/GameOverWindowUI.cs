using UnityEngine;

public class GameOverWindowUI : MessageWindowUI
{
    [SerializeField] public TwoPushButton restartButton = default;
    [SerializeField] public TwoPushButton titleButton = default;
    [SerializeField] private SelectIcon unityChanIcon = default;

    private ButtonsHandler handler;

    protected override void Start()
    {
        handler = new ButtonsHandler(unityChanIcon, restartButton, titleButton);
        handler.Inactivate();

        Inactivate();
    }

    public void ActivateButtons() => handler.Activate(true);
}
