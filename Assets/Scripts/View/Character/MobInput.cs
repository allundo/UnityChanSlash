using UnityEngine;
using UniRx;

[RequireComponent(typeof(MobCommander))]
[RequireComponent(typeof(MapUtil))]
public abstract class MobInput : MonoBehaviour
{
    protected MobCommander commander;

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
    /// This method is called by Start(). Override it to customize commands' behavior.
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

    protected virtual void Update()
    {
        if (GameManager.Instance.isPaused) return;

        InputCommand(GetCommand());
    }

    public virtual void InputCommand(Command cmd)
    {
        if (!isCommandValid) return;

        commander.EnqueueCommand(cmd);
    }

    public void ForceEnqueue(Command cmd, bool dispatch) => commander.EnqueueCommand(cmd, dispatch);

    protected abstract Command GetCommand();

    public virtual void InputDie()
    {
        commander.ClearAll();
        ForceEnqueue(die, true);
    }

    public virtual void ValidateInput(bool isValid)
    {
        isCommandValid = isValid;
    }
}
