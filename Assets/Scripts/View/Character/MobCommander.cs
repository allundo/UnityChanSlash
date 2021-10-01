using UnityEngine;
using System;
using UniRx;
using System.Collections.Generic;

[RequireComponent(typeof(MobAnimator))]
[RequireComponent(typeof(MapUtil))]
public abstract class MobCommander : MonoBehaviour
{
    public MobAnimator anim { get; protected set; }

    public bool IsIdling => currentCommand == null;
    public bool IsDie => currentCommand is DieCommand;

    protected Queue<Command> cmdQueue = new Queue<Command>();
    public Command currentCommand { get; protected set; } = null;

    public MapUtil map { get; protected set; } = default;

    public ISubject<Unit> onDead { get; protected set; } = new Subject<Unit>();
    public IObservable<Unit> OnDead => onDead;

    public ISubject<Unit> onCompleted { get; protected set; } = new Subject<Unit>();
    protected IObservable<Unit> OnCompleted => onCompleted;

    public ISubject<bool> onValidateInput { get; protected set; } = new Subject<bool>();
    public IObservable<bool> OnValidateInput => onValidateInput;

    protected virtual void Awake()
    {
        anim = GetComponent<MobAnimator>();
        map = GetComponent<MapUtil>();
    }

    protected virtual void Start()
    {
        OnCompleted.Subscribe(_ => DispatchCommand()).AddTo(this);
    }

    public virtual void EnqueueCommand(Command cmd) => EnqueueCommand(cmd, IsIdling);

    public virtual void EnqueueCommand(Command cmd, bool dispatch = false)
    {
        if (cmd == null) return;

        cmdQueue.Enqueue(cmd);
        onValidateInput.OnNext(false);

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

    public virtual void Inactivate()
    {
        currentCommand = null;
    }

    public virtual void ClearAll()
    {
        cmdQueue.Clear();
        currentCommand?.Cancel();
    }
}
