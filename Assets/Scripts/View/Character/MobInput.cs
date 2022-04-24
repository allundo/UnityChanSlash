using UnityEngine;

public interface IMobInput : IInput
{
    void InputIced(float duration);
    void OnIceCrash();
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

    public virtual void OnIceCrash()
    {

#if UNITY_EDITOR
        if (!(commander.currentCommand is IcedCommand))
        {
            Debug.Log("IcedCrash(): " + gameObject.name + " isn't iced!, Command: " + commander.currentCommand, gameObject);
        }
#endif

        commander.currentCommand.Cancel();
    }

    protected virtual ICommand GetIcedCommand(float duration)
        => new IcedCommand(target, duration);
}
