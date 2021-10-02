using UnityEngine;
using UniRx;

/// <summary>
/// Inputs Command to Command queue in Commander. <br />
/// </summary>
[RequireComponent(typeof(MobCommander))]
[RequireComponent(typeof(MapUtil))]
public abstract class MobInput : MonoBehaviour
{
    /// <summary>
    /// Target Commander to input Command.
    /// </summary>
    protected MobCommander commander;

    /// <summary>
    /// Stops input Command if false. <br />
    /// Set this validate flag via commander.OnValidateInput notification.<br />
    /// This is invalidated at executing Command and validated again<br />
    /// on the middle of the execution.
    /// </summary>
    public bool isCommandValid { get; protected set; } = true;

    protected Command die = null;

    public MapUtil map { get; protected set; } = default;

    protected virtual void Awake()
    {
        commander = GetComponent<MobCommander>();
        map = GetComponent<MapUtil>();
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
        die = new DieCommand(commander, 0.1f);
    }

    /// <summary>
    /// This method is called by Start(). Override it to customize commands' behavior.
    /// </summary>
    protected virtual void Subscribe()
    {
        commander.OnValidateInput.Subscribe(isValid => ValidateInput(isValid)).AddTo(this);
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
        if (!isCommandValid) return;

        commander.EnqueueCommand(cmd);
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

    public virtual void ValidateInput(bool isValid)
    {
        isCommandValid = isValid;
    }
}
