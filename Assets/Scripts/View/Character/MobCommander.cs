using UnityEngine;
using System;
using UniRx;
using System.Collections.Generic;

/// <summary>
/// Handles Command queuing and dispatching. <br />
/// Keeps attached component data used by Command execution.
/// </summary>
[RequireComponent(typeof(MobAnimator))]
[RequireComponent(typeof(MapUtil))]
public abstract class MobCommander : MonoBehaviour
{
    /// <summary>
    /// Animation handler for Command execution.
    /// </summary>
    public MobAnimator anim { get; protected set; }

    /// <summary>
    /// Direction related data for Command execution.
    /// </summary>
    public MapUtil map { get; protected set; } = default;

    public bool IsIdling => currentCommand == null;
    public bool IsDie => currentCommand is DieCommand;

    /// <summary>
    /// Reserve Commands to execute in order.
    /// </summary>
    /// <typeparam name="Command">Command that Execute() method is implemented </typeparam>
    protected Queue<Command> cmdQueue = new Queue<Command>();

    /// <summary>
    /// Executing Command. null if no Command is executing.
    /// </summary>
    public Command currentCommand { get; protected set; } = null;

    /// <summary>
    /// Notify the end of DieCommand execution.
    /// </summary>
    public ISubject<Unit> onDead { get; protected set; } = new Subject<Unit>();
    public IObservable<Unit> OnDead => onDead;

    /// <summary>
    /// Notify the end of Command execution. Mainly use for next Command dispatching.
    /// </summary>
    public ISubject<Unit> onCompleted { get; protected set; } = new Subject<Unit>();
    protected IObservable<Unit> OnCompleted => onCompleted;

    /// <summary>
    /// Notification for validating input.
    /// </summary>
    /// <typeparam name="bool">true: validate, false: invalidate</typeparam>
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

    /// <summary>
    /// Enqueue a Command and invalidate following input.
    /// </summary>
    /// <param name="cmd">Command to enqueue</param>
    /// <param name="dispatch">Dispatch Command immediately if true. TODO: This is not Command interaption for now.</param>
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

    /// <summary>
    /// Dequeue a Command and execute it.
    /// </summary>
    /// <returns>true if Command is present and executed</returns>
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

    /// <summary>
    /// Clean up process on inactivation.
    /// </summary>
    public virtual void Inactivate()
    {
        currentCommand = null;
    }

    /// <summary>
    /// Clear all Command queue and cancel current executing Command.
    /// </summary>
    public virtual void ClearAll()
    {
        cmdQueue.Clear();
        currentCommand?.Cancel();
    }
}
