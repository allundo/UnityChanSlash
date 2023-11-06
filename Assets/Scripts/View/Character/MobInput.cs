using UnityEngine;

public interface IMobInput : IInput
{
    ICommand InputIced(float icingFrames);
    void OnIceCrash();
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

    public virtual ICommand InputIced(float icingFrames)
    {
        DisableInput();

        if (IsIdling) return ForceEnqueue(GetIcedCommand(icingFrames, 0.98f));

        // Retrieve remaining process of current command
        ICommand continuation = commander.PostponeCurrent();

        ClearAll();

        ICommand iced = ForceEnqueue(GetIcedCommand(icingFrames, 1f));

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

        if (currentCommand is IcedCommand) commander.Cancel();
    }

    protected virtual ICommand GetIcedCommand(float duration, float validateTiming)
        => new IcedCommand(target, duration, validateTiming);

    public float GetIcingFrames() => IsIced ? (currentCommand as IIcedCommand).framesToMelt : 0.0f;
}
