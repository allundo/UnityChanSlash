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

        // Retrieve remaining process of current command
        ICommand continuation = commander.PostponeCurrent();

        ForceEnqueue(GetIcedCommand(duration));

        // Enqueue remaining process command after icing
        if (continuation != null) ForceEnqueue(continuation);

        DisableInput();
    }

    public virtual void OnIceCrash()
    {

#if UNITY_EDITOR
        ICommand cmd = commander.currentCommand;
        bool isCurrentlyIced = cmd is IcedCommand || cmd is RabbitIcedFall || cmd is FlyingIcedFall;
        if (!isCurrentlyIced)
        {
            Debug.Log("IcedCrash(): " + gameObject.name + " isn't iced!, Command: " + cmd, gameObject);
        }
#endif

        commander.Cancel();
    }

    protected virtual ICommand GetIcedCommand(float duration)
        => new IcedCommand(target, duration);
}
