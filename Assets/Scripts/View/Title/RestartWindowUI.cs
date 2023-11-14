using UnityEngine;

public class RestartWindowUI : MessageWindowBase
{
    [SerializeField] public TwoPushButton restartButton = default;
    [SerializeField] public TwoPushButton titleButton = default;
    [SerializeField] private SelectIcon unityChanIcon = default;

    private ButtonsHandler handler;

    protected virtual void Start()
    {
        handler = new ButtonsHandler(unityChanIcon, restartButton, titleButton);
        handler.Inactivate();
    }

    public void ActivateButtons() => handler.Activate(true);
    public void InactivateButtons() => handler.Inactivate();
}
