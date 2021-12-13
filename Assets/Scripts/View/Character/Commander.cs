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
    protected LinkedList<Command> cmdQueue = new LinkedList<Command>();

    /// <summary>
    /// Executing Command. null if no Command is executing.
    /// </summary>
    public virtual Command currentCommand { get; protected set; } = null;

    /// <summary>
    /// Command reserved next. null if no Command is queued.
    /// </summary>
    public virtual Command NextCommand => cmdQueue.First?.Value;

    protected IDisposable execDisposable = null;

    /// <summary>
    /// Enqueue a Command and invalidate following input.
    /// </summary>
    /// <param name="cmd">Command to enqueue</param>
    /// <param name="dispatch">Dispatch Command immediately if true. TODO: This is not Command interaption for now.</param>
    public virtual void EnqueueCommand(Command cmd)
    {
        cmdQueue.Enqueue(cmd);

        if (IsIdling)
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
    /// Reserve new Command at first of command queue to execute immediately.
    /// </summary>
    /// <param name="cmd">New Command to execute as interruption</param>
    /// <param name="isCancel">Cancels current executing command if TRUE</param>
    public void Interrupt(Command cmd, bool isCancel = true)
    {
        cmdQueue.AddFirst(cmd);

        if (isCancel) Cancel();

        if (cmdQueue.Count > 0) currentCommand.CancelValidate();
    }

    public void ReplaceNext(Command cmd)
    {
        cmdQueue.ReplaceFirstWith(cmd);
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
        DispatchCommand();
    }

    public void CancelValidate()
    {
        currentCommand?.CancelValidate();
    }
}
