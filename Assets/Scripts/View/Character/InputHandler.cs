using UnityEngine;

public interface IInput
{
    bool isCommandValid { get; }
    bool IsFightValid { get; }
    ICommand InputCommand(ICommand cmd);
    ICommand ForceEnqueue(ICommand cmd);
    ICommand Interrupt(ICommand cmd, bool isCancel = true, bool isQueueClear = false);
    ICommand InterruptDie();
    ICommand currentCommand { get; }
    void ValidateInput(bool isTriggerOnly = false);
    void DisableInput(bool isTriggerOnly = false);
    void OnActive();
    void ClearAll(bool isQueueOnly = false, bool isValidInput = false, int threshold = 100);
}

/// <summary>
/// Inputs ICommand to ICommand queue in Commander. <br />
/// </summary>
[RequireComponent(typeof(CommandTarget))]
[RequireComponent(typeof(MapUtil))]
public abstract class InputHandler : MonoBehaviour, IInput
{
    /// <summary>
    /// Target Commander to input Command.
    /// </summary>
    public Commander commander { get; protected set; }
    protected ICommandTarget target;

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
        commander = new Commander(gameObject);
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
        if (TimeManager.Instance.isPaused) return;

        if (isCommandValid) InputCommand(GetCommand());
    }

    /// <summary>
    /// Input a ICommand to MobCommander while isCommandValid flag allows.
    /// </summary>
    /// <param name="cmd">Command to input</param>
    public virtual ICommand InputCommand(ICommand cmd)
    {
        if (cmd == null) return null;

        isCommandValid = false;

        commander.EnqueueCommand(cmd);
        return cmd;
    }

    /// <summary>
    /// Shorthand enqueue
    /// </summary>
    public ICommand ForceEnqueue(ICommand cmd)
    {
        commander.EnqueueCommand(cmd);
        return cmd;
    }

    /// <summary>
    /// Execute command immediately without queuing
    /// </summary>
    public ICommand Interrupt(ICommand cmd, bool isCancel = true, bool isQueueClear = false)
    {
        DisableInput();
        commander.Interrupt(cmd, isCancel, isQueueClear);
        return cmd;
    }

    protected abstract ICommand GetCommand();

    public ICommand currentCommand => commander.currentCommand;

    public virtual ICommand InterruptDie() => Interrupt(die, true, true);

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

    public virtual void ClearAll(bool isQueueOnly = false, bool isValidInput = false, int threshold = 100)
    {
        commander.ClearAll(isQueueOnly, isValidInput);
        isCommandValid = isValidInput;
    }
}
