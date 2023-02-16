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

    protected virtual bool IsIced => currentCommand is IIcedCommand;

    protected override void Awake()
    {
        base.Awake();
        mobMap = map as IMobMapUtil;
    }

    public virtual ICommand InputIced(float duration)
    {
        DisableInput();

        if (IsIdling) return ForceEnqueue(GetIcedCommand(duration, 0.98f));

        // Retrieve remaining process of current command
        ICommand continuation = commander.PostponeCurrent();

        ClearAll();

        ICommand iced = ForceEnqueue(GetIcedCommand(duration, 1f));

        // Enqueue remaining process of interrupted command after icing
        ForceEnqueue(continuation);

        return iced;
    }

    public virtual void OnIceCrash()
    {

#if UNITY_EDITOR
        if (!IsIced)
        {
            Debug.Log("IcedCrash(): " + gameObject.name + " isn't iced!, Command: " + currentCommand, gameObject);
        }
#endif

        commander.Cancel();
    }

    protected virtual ICommand GetIcedCommand(float duration, float validateTiming)
        => new IcedCommand(target, duration, validateTiming);

    public float GetIcingFrames() => IsIced ? currentCommand.RemainingFramesToComplete : 0.0f;
}
