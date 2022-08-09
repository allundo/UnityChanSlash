using UnityEngine;

public interface ITargetingUI
{
    void EnableChargingUp();
    void DisableChargingUp();

    void Show(Vector2 pos);
    void Hide();
}

public abstract class TargetingUI : FadeColor, ITargetingUI
{
    [SerializeField] private Color normalColor = default;
    [SerializeField] private Color chargingColor = default;

    protected override Color DefaultColor() => normalColor;
    protected override Color ChangedColor() => chargingColor;

    public virtual void EnableChargingUp()
    {
        SwitchColor(true);
    }

    public virtual void DisableChargingUp()
    {
        SwitchColor(false);
    }
}