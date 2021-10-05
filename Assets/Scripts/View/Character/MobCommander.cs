using UnityEngine;
using System;
using UniRx;
using System.Collections.Generic;

/// <summary>
/// Handles Command queuing and dispatching. <br />
/// Keeps attached component data used by Command execution.
/// </summary>
public class MobCommander
{
    public MobCommander(CommandTarget commandTarget)
    {
        targetObject = commandTarget.gameObject;
    }

    protected GameObject targetObject;

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
    protected IDisposable execDisposable = null;

    /// <summary>
    /// Notify the end of DieCommand execution.
    /// </summary>
    public ISubject<Unit> onDead { get; protected set; } = new Subject<Unit>();
    public IObservable<Unit> OnDead => onDead;

    /// <summary>
    /// Notification for validating input.
    /// </summary>
    /// <typeparam name="bool">true: validate, false: invalidate</typeparam>
    public ISubject<bool> onValidateInput { get; protected set; } = new Subject<bool>();
    public IObservable<bool> OnValidateInput => onValidateInput;

    public virtual void EnqueueCommand(Command cmd) => EnqueueCommand(cmd, IsIdling);

    /// <summary>
    /// Enqueue a Command and invalidate following input.
    /// </summary>
    /// <param name="cmd">Command to enqueue</param>
    /// <param name="dispatch">Dispatch Command immediately if true. TODO: This is not Command interaption for now.</param>
    public virtual void EnqueueCommand(Command cmd, bool dispatch = false)
    {
        cmdQueue.Enqueue(cmd);

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

            Subscribe(currentCommand.Execute());
            return true;
        }

        currentCommand = null;
        return false;
    }

    protected virtual void Subscribe(IObservable<bool> executionObservable)
    {
        if (executionObservable == null) return;

        execDisposable =
            executionObservable.Subscribe(
                isTriggerOnly => onValidateInput.OnNext(isTriggerOnly),
                () => DispatchCommand()
            )
            .AddTo(targetObject);
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
        Cancel();
    }
    public virtual void Cancel()
    {
        currentCommand?.Cancel();
        execDisposable?.Dispose();
        onValidateInput.OnNext(false);
        DispatchCommand();
    }
}
