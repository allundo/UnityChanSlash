using UnityEngine;
using System;
using UniRx;
using System.Collections.Generic;

[RequireComponent(typeof(MobAnimator))]
[RequireComponent(typeof(MapUtil))]
public abstract partial class MobCommander : MonoBehaviour
{
    public MobAnimator anim { get; protected set; }

    protected bool isCommandValid = true;

    public bool IsIdling => currentCommand == null;

    protected Queue<Command> cmdQueue = new Queue<Command>();
    protected Command currentCommand = null;
    protected Command die = null;

    public MapUtil map { get; protected set; } = default;

    private ISubject<Unit> onDead = new Subject<Unit>();
    public IObservable<Unit> OnDead => onDead;

    protected ISubject<Unit> onCompleted = new Subject<Unit>();
    protected IObservable<Unit> OnCompleted => onCompleted;

    protected ISubject<bool> onValidated = new Subject<bool>();
    protected IObservable<bool> OnValidated => onValidated;

    protected virtual void Awake()
    {
        anim = GetComponent<MobAnimator>();
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
        die = new DieCommand(this, 0.1f);
    }

    /// <summary>
    /// This method is called by Start(). Override it to customize commands' behavior.
    /// </summary>
    protected virtual void Subscribe()
    {
        OnCompleted.Subscribe(_ => DispatchCommand()).AddTo(this);
        OnValidated.Subscribe(_ => ValidateInput()).AddTo(this);
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
