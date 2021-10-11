using UnityEngine;
using System;
using UniRx;
using System.Collections.Generic;

/// <summary>
/// Handles Command queuing and dispatching.
/// </summary>
public class Commander
{
    public Commander(CommandTarget commandTarget)
    {
        targetObject = commandTarget.gameObject;
    }

    /// <summary>
    /// Used only for detecting destraction of command target object
    /// </summary>
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
    /// Notification for validating input.
    /// </summary>
    /// <typeparam name="bool">true: validate, false: invalidate</typeparam>
    public ISubject<bool> onValidateInput { get; protected set; } = new Subject<bool>();
    public IObservable<bool> OnValidateInput => onValidateInput;

    public void EnqueueCommand(Command cmd) => EnqueueCommand(cmd, IsIdling);

    /// <summary>
    /// Enqueue a Command and invalidate following input.
    /// </summary>
    /// <param name="cmd">Command to enqueue</param>
    /// <param name="dispatch">Dispatch Command immediately if true. TODO: This is not Command interaption for now.</param>
    public void EnqueueCommand(Command cmd, bool dispatch = false)
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
    protected bool DispatchCommand()
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

    protected void Subscribe(IObservable<bool> execObservable)
    {
        if (execObservable == null) return;

        execDisposable =
            execObservable.Subscribe(
                isTriggerOnly => onValidateInput.OnNext(isTriggerOnly),
                () => DispatchCommand()
            )
            .AddTo(targetObject);
    }

    /// <summary>
    /// Clear all Command queue and cancel current executing Command.
    /// </summary>
    public void ClearAll()
    {
        cmdQueue.Clear();
        Cancel();
    }

    public void Cancel()
    {
        currentCommand?.Cancel();
        execDisposable?.Dispose();
        onValidateInput.OnNext(false);
        DispatchCommand();
    }
}
