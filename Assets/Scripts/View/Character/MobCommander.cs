using UnityEngine;
using UniRx;
using System.Collections.Generic;

[RequireComponent(typeof(MobAnimator))]
[RequireComponent(typeof(MapUtil))]
public abstract partial class MobCommander : MonoBehaviour
{
    public MobAnimator anim { get; protected set; }
    protected MobReactor reactor;

    protected bool isCommandValid = true;

    public bool IsIdling => currentCommand == null;

    protected Queue<Command> cmdQueue = new Queue<Command>();
    public Command currentCommand { get; protected set; } = null;
    protected Command die = null;

    public MapUtil map { get; protected set; } = default;

    protected virtual void Awake()
    {
        reactor = GetComponent<MobReactor>();
        anim = GetComponent<MobAnimator>();
        map = GetComponent<MapUtil>();
    }

    protected virtual void Start()
    {
        SetCommands();
    }

    /// <summary>
    /// This method is called by Start(). Override it to customize commands' behavior.
    /// </summary>
    protected virtual void SetCommands()
    {
        die = new DieCommand(this, 0.1f);
    }

    protected virtual void Update()
    {
        if (GameManager.Instance.isPaused) return;

        Execute(GetCommand());
    }

    protected virtual void Execute(Command cmd)
    {
        if (!isCommandValid) return;


        EnqueueCommand(cmd, IsIdling);
    }

    protected virtual void EnqueueCommand(Command cmd, bool dispatch = false)
    {
        if (cmd == null) return;

        cmdQueue.Enqueue(cmd);
        InvalidateInput();

        if (dispatch)
        {
            DispatchCommand();
        }
    }

    protected virtual bool DispatchCommand()
    {
        if (cmdQueue.Count > 0)
        {
            currentCommand = cmdQueue.Dequeue();
            currentCommand.Execute();
            return true;
        }

        currentCommand = null;
        return false;
    }

    protected abstract Command GetCommand();

    public virtual void SetDie()
    {
        map.ResetOnCharactor();

        cmdQueue.Clear();
        currentCommand?.Cancel();
        EnqueueDie();
    }

    protected virtual void EnqueueDie()
    {
        EnqueueCommand(die, true);
    }

    public virtual void Activate()
    {
        ValidateInput();
    }

    public virtual void Inactivate()
    {
        currentCommand = null;
    }

    protected virtual void ValidateInput()
    {
        isCommandValid = true;
    }

    protected virtual void InvalidateInput()
    {
        isCommandValid = false;
    }
}
