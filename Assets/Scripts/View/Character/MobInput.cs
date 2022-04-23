using UnityEngine;

public interface IMobInput : IInput
{
    void InputIced(float duration);
}

public interface IUndeadInput : IMobInput
{
    void InputSleep();
}

/// <summary>
/// Inputs ICommand to ICommand queue in Commander. <br />
/// </summary>
[RequireComponent(typeof(CommandTarget))]
[RequireComponent(typeof(MobMapUtil))]
public abstract class MobInput : InputHandler, IMobInput
{
    protected IMobMapUtil mobMap;
    protected override void Awake()
    {
        base.Awake();
        mobMap = map as IMobMapUtil;
    }

    public virtual void InputIced(float duration)
    {
        // Delete Command queue only
        ClearAll(true);
        ICommand continuation = commander.PostponeCurrent();
        ForceEnqueue(GetIcedCommand(duration));
        if (continuation != null) ForceEnqueue(continuation);
        DisableInput();
    }

    protected virtual ICommand GetIcedCommand(float duration)
        => new IcedCommand(target, duration);
}
