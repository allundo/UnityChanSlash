using UnityEngine;

public interface IMobInput : IInput
{
    ICommand InputIced(float duration);
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

    protected virtual bool IsIced => currentCommand is IcedCommand;

    protected override void Awake()
    {
        base.Awake();
        mobMap = map as IMobMapUtil;
    }

    public virtual ICommand InputIced(float duration)
    {
        // Delete Command queue only
        ClearAll(true);

        // Retrieve remaining process of current command
        ICommand continuation = commander.PostponeCurrent();

        ICommand iced = GetIcedCommand(duration);

        ForceEnqueue(iced);

        // Enqueue remaining process command after icing
        if (continuation != null) ForceEnqueue(continuation);

        DisableInput();

        return iced;
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

    public float GetIcingFrames() => IsIced ? currentCommand.RemainingFramesToComplete : 0.0f;
}
