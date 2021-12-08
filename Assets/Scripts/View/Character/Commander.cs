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

    /// <summary>
    /// Reserve Commands to execute in order.
    /// </summary>
    /// <typeparam name="Command">Command that Execute() method is implemented </typeparam>
    protected Queue<Command> cmdQueue = new Queue<Command>();

    /// <summary>
    /// Executing Command. null if no Command is executing.
    /// </summary>
    public virtual Command currentCommand { get; protected set; } = null;

    protected IDisposable execDisposable = null;

    public void EnqueueCommand(Command cmd) => EnqueueCommand(cmd, IsIdling);

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
    protected bool DispatchCommand()
    {
        if (cmdQueue.Count > 0)
        {
            SetAndExec(cmdQueue.Dequeue());
            return true;
        }

        currentCommand = null;
        return false;
    }

    protected void SetAndExec(Command cmd)
    {
        currentCommand = cmd;
        Subscribe(cmd.Execute());
    }

    protected virtual void Subscribe(IObservable<Unit> execObservable)
    {
        if (execObservable == null) return;

        execDisposable = execObservable.Subscribe(null, () => DispatchCommand()).AddTo(targetObject);
    }

    /// <summary>
    /// Replace current Command with a new Command and execute immediately.
    /// </summary>
    /// <param name="cmd">New Command to execute as interruption</param>
    public void Interrupt(Command cmd)
    {
        Cancel(false);
        SetAndExec(cmd);
        if (cmdQueue.Count > 0) cmd.CancelValidate();
    }

    /// <summary>
    /// Clear all Command queue and cancel current executing Command.
    /// </summary>
    public void ClearAll()
    {
        cmdQueue.Clear();
        Cancel();
    }

    public void Cancel(bool dispatch = true)
    {
        currentCommand?.Cancel();
        execDisposable?.Dispose();
        if (dispatch) DispatchCommand();
    }

    public void CancelValidate()
    {
        currentCommand?.CancelValidate();
    }
}
