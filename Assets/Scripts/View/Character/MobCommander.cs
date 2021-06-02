using UnityEngine;
using UniRx;
using System.Collections.Generic;

[RequireComponent(typeof(MobAnimator))]
[RequireComponent(typeof(MapUtil))]
public abstract partial class MobCommander : MonoBehaviour
{
    public MobAnimator anim { get; protected set; }
    protected MobReactor reactor;

    protected bool isCommandValid
    {
        get
        {
            return IsCommandValid.Value;
        }
        set
        {
            IsCommandValid.Value = value;
        }
    }
    protected IReactiveProperty<bool> IsCommandValid;
    protected ReactiveCommand<Command> InputReactive;

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

        IsCommandValid = new ReactiveProperty<bool>(true);
        InputReactive = new ReactiveCommand<Command>(IsCommandValid);

    }

    protected virtual void Start()
    {
        SetCommands();

        InputReactive.Subscribe(com => EnqueueCommand(com, IsIdling)).AddTo(this);
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
        InputReactive.Execute(GetCommand());
    }

    protected virtual void EnqueueCommand(Command cmd, bool dispatch = false)
    {
        if (cmd == null) return;

        cmdQueue.Enqueue(cmd);
        isCommandValid = false;

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
        isCommandValid = true;
    }

    public virtual void Inactivate()
    {
        currentCommand = null;
    }
}
