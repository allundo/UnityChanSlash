using UnityEngine;
using UniRx;

/// <summary>
/// Inputs Command to Command queue in Commander. <br />
/// </summary>
[RequireComponent(typeof(CommandTarget))]
[RequireComponent(typeof(MapUtil))]
public abstract class MobInput : MonoBehaviour
{
    /// <summary>
    /// Target Commander to input Command.
    /// </summary>
    public MobCommander commander { get; protected set; }
    public CommandTarget target { get; protected set; }

    /// <summary>
    /// Stops input Command if false. <br />
    /// Set this validate flag via commander.OnValidateInput notification.<br />
    /// This is invalidated at executing Command and validated again<br />
    /// on the middle of the execution.
    /// </summary>
    public bool isCommandValid { get; protected set; } = true;

    protected Command die = null;

    protected MapUtil map;

    protected virtual void Awake()
    {
        target = GetComponent<CommandTarget>();
        map = GetComponent<MapUtil>();
        commander = new MobCommander(target);
    }

    protected virtual void Start()
    {
        SetCommands();
        Subscribe();
    }

    /// <summary>
    /// This method is called by Start(). Override it to customize commands' behavior. <br />
    /// Mainly used for Commands definition.
    /// </summary>
    protected virtual void SetCommands()
    {
        die = new DieCommand(target, 0.1f);
    }

    /// <summary>
    /// This method is called by Start(). Override it to customize commands' behavior.
    /// </summary>
    protected virtual void Subscribe()
    {
        commander.OnValidateInput
            .Subscribe(isTriggerOnly => ValidateInput(isTriggerOnly))
            .AddTo(this);
    }

    /// <summary>
    /// Input a Command automatically for every frames.
    /// </summary>
    protected virtual void Update()
    {
        if (GameManager.Instance.isPaused) return;

        InputCommand(GetCommand());
    }

    /// <summary>
    /// Input a Command to MobCommander while isCommandValid flag allows.
    /// </summary>
    /// <param name="cmd">Command to input</param>
    public virtual void InputCommand(Command cmd)
    {
        if (!isCommandValid || cmd == null) return;

        commander.EnqueueCommand(cmd);
        isCommandValid = false;
    }

    /// <summary>
    /// Shorthand enqueue
    /// </summary>
    public void ForceEnqueue(Command cmd, bool dispatch) => commander.EnqueueCommand(cmd, dispatch);

    protected abstract Command GetCommand();

    public virtual void InputDie()
    {
        // Clear all queuing Commands to execute DieCommand immediately.
        commander.ClearAll();

        ForceEnqueue(die, true);
    }

    public virtual void ValidateInput(bool isTriggerOnly = false)
    {
        isCommandValid = true;
    }
}
