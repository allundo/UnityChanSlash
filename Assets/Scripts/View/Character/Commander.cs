using UnityEngine;
using System;
using UniRx;
using System.Collections.Generic;

/// <summary>
/// Handles Command queuing and dispatching.
/// </summary>
public class Commander
{
    public Commander(GameObject target)
    {
        targetObject = target;
    }

    /// <summary>
    /// Used only for detecting destruction of command target object
    /// </summary>
    protected GameObject targetObject;

    public bool IsIdling => currentCommand == null;

    /// <summary>
    /// Reserve Commands to execute in order.
    /// </summary>
    /// <typeparam name="ICommand">Command that Execute() method is implemented </typeparam>
    protected LinkedList<ICommand> cmdQueue = new LinkedList<ICommand>();

    public int QueueCount => cmdQueue.Count;

    /// <summary>
    /// Executing Command. null if no Command is executing.
    /// </summary>
    public virtual ICommand currentCommand { get; protected set; } = null;

    /// <summary>
    /// Command reserved next. null if no Command is queued.
    /// </summary>
    public virtual ICommand NextCommand => cmdQueue.First?.Value;

    protected IDisposable execDisposable = null;

    /// <summary>
    /// Enqueue a Command and invalidate following input.
    /// </summary>
    /// <param name="cmd">Command to enqueue</param>
    public virtual void EnqueueCommand(ICommand cmd)
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

    protected virtual void SetAndExec(ICommand cmd)
    {
        currentCommand = cmd;
        Subscribe(cmd);
    }

    protected virtual void Subscribe(ICommand cmd)
    {
        IObservable<Unit> execObservable = cmd.Execute();

        if (execObservable == null) return;

        execDisposable = execObservable.Subscribe(null, () => DispatchCommand()).AddTo(targetObject);
    }

    /// <summary>
    /// Reserve new Command at first in command queue to execute immediately.
    /// </summary>
    /// <param name="cmd">New Command to execute as interruption</param>
    /// <param name="isCancel">Cancels current executing command if TRUE</param>
    /// <param name="isQueueClear">Clear Command queued after interruption if TRUE</param>
    public void Interrupt(ICommand cmd, bool isCancel = true, bool isQueueClear = false)
    {
        if (isQueueClear) cmdQueue.Clear();

        InsertQueue(cmd);

        if (IsIdling)
        {
            DispatchCommand();
        }
        else
        {
            currentCommand?.CancelValidate();
            if (isCancel) Cancel();
        }
    }

    /// <summary>
    /// Pauses current command and dispatches next command.
    /// </summary>
    /// <returns>New command including remaining process of current command.</returns>
    public ICommand PostponeCurrent()
    {
        ICommand continuation = currentCommand?.GetContinuation();
        execDisposable?.Dispose();
        DispatchCommand(); // No command is dispatched if iced or command queue is empty.
        return continuation;
    }

    protected virtual void InsertQueue(ICommand cmd) => cmdQueue.AddFirst(cmd);

    public void ReplaceNext(ICommand cmd)
    {
        cmdQueue.ReplaceFirstWith(cmd);
    }

    /// <summary>
    /// Clear all Command queue and cancel current executing Command.
    /// </summary>
    /// <param name="isQueueOnly">Doesn't cancel current Command if true</param>
    /// <param name="isValidInput">Cancels validate tween of current Command if false</param>
    /// <param name="threshold">Not used. Used only by PlayerCommander for now.</param>
    public virtual void ClearAll(bool isQueueOnly = false, bool isValidInput = false, int threshold = 100)
    {
        cmdQueue.Clear();

        if (isQueueOnly)
        {
            if (!isValidInput) currentCommand?.CancelValidate();
        }
        else
        {
            Cancel();
        }
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
