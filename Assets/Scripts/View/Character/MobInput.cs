using UnityEngine;

public interface IInput
{
    Commander commander { get; }
    bool isCommandValid { get; }
    bool IsFightValid { get; }
    void InputCommand(ICommand cmd);
    void ForceEnqueue(ICommand cmd);
    void Interrupt(ICommand cmd, bool isCancel = true);
    void InputDie();
    void ValidateInput(bool isTriggerOnly = false);
    void DisableInput(bool isTriggerOnly = false);
    void OnActive();
    void ClearAll(bool isQueueOnly = false, bool isValidInput = false);
}

/// <summary>
/// Inputs ICommand to ICommand queue in Commander. <br />
/// </summary>
[RequireComponent(typeof(CommandTarget))]
[RequireComponent(typeof(MapUtil))]
public abstract class MobInput : MonoBehaviour, IInput
{
    /// <summary>
    /// Target Commander to input Command.
    /// </summary>
    public Commander commander { get; protected set; }
    protected CommandTarget target;

    /// <summary>
    /// Stops input ICommand if false. <br />
    /// Set this validate flag via commander.OnValidateInput notification.<br />
    /// This is invalidated at executing ICommand and validated again<br />
    /// on the middle of the execution.
    /// </summary>
    public bool isCommandValid { get; protected set; } = true;

    protected bool IsIdling => commander.IsIdling;
    public virtual bool IsFightValid => IsIdling;

    protected ICommand die = null;

    protected IMapUtil map;

    protected virtual void Awake()
    {
        target = GetComponent<CommandTarget>();
        map = GetComponent<MapUtil>();

        SetCommander();
        SetCommands();
    }

    protected virtual void SetCommander()
    {
        commander = new Commander(target);
    }

    /// <summary>
    /// This method is called by Awake(). Override it to customize commands' behavior. <br />
    /// Mainly used for Commands definition.
    /// </summary>
    protected virtual void SetCommands()
    {
        die = new DieCommand(target, 3.6f);
    }

    /// <summary>
    /// Input a Command automatically for every frames.
    /// </summary>
    protected virtual void Update()
    {
        if (GameManager.Instance.isPaused) return;

        if (isCommandValid) InputCommand(GetCommand());
    }

    /// <summary>
    /// Input a ICommand to MobCommander while isCommandValid flag allows.
    /// </summary>
    /// <param name="cmd">Command to input</param>
    public virtual void InputCommand(ICommand cmd)
    {
        if (cmd == null) return;

        isCommandValid = false;

        commander.EnqueueCommand(cmd);
    }

    /// <summary>
    /// Shorthand enqueue
    /// </summary>
    public void ForceEnqueue(ICommand cmd) => commander.EnqueueCommand(cmd);

    /// <summary>
    /// Execute command immediately without queuing
    /// </summary>
    public void Interrupt(ICommand cmd, bool isCancel = true)
    {
        DisableInput();
        commander.Interrupt(cmd, isCancel);
    }

    protected abstract ICommand GetCommand();

    public virtual void InputDie()
    {
        // Clear all queuing Commands to execute DieCommand immediately.
        ClearAll();

        Interrupt(die);
    }

    public virtual void ValidateInput(bool isTriggerOnly = false)
    {
        // Don't allow multiple Command queuing for now
        if (commander.QueueCount == 0) isCommandValid = true;
    }

    public virtual void DisableInput(bool isTriggerOnly = false)
    {
        isCommandValid = false;
    }

    public virtual void OnActive()
    {
        ValidateInput();
    }

    public virtual void ClearAll(bool isQueueOnly = false, bool isValidInput = false)
    {
        commander.ClearAll(isQueueOnly, isValidInput);
        isCommandValid = isValidInput;
    }
}
