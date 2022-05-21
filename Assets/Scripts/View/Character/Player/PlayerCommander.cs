using UniRx;
using System;
using System.Linq;
using System.Collections.Generic;

public class PlayerCommander : ShieldCommander
{
    protected PlayerAnimator anim;

    protected ISubject<ICommand> commandComplete = new Subject<ICommand>();
    public IObservable<ICommand> CommandComplete => commandComplete;

    public PlayerCommander(PlayerCommandTarget target) : base(target.gameObject)
    {
        anim = target.anim as PlayerAnimator;
    }

    public override void EnqueueCommand(ICommand cmd)
    {
        // Don't allow over queuing of GuardCommand
        if (cmdQueue.First?.Value is GuardCommand) cmdQueue.Dequeue();

        base.EnqueueCommand(cmd);
        if (anim.cancel.Bool) CheckCancel(); // Cancel current cancelable Attack if newly enqueued command is Attack
    }

    protected override void Subscribe(ICommand cmd)
    {
        IObservable<Unit> execObservable = cmd.Execute();

        if (execObservable == null) return;

        execDisposable = execObservable
            .Subscribe(
                _ => CheckCancel(), // Cancel current Attack if next is also Attack on cancellation timing
                () =>
                {
                    DispatchCommand();
                    commandComplete.OnNext(cmd);
                }
            )
            .AddTo(targetObject);
    }

    protected void CheckCancel()
    {
        if (cmdQueue.First?.Value is PlayerAttack)
        {
            Cancel();
        }
    }

    protected override void InsertQueue(ICommand cmd)
    {
        for (var node = cmdQueue.First; node != null; node = node.Next)
        {
            if (!node.Value.IsPriorTo(cmd))
            {
                cmdQueue.AddBefore(node, new LinkedListNode<ICommand>(cmd));
                return;
            }
        }

        cmdQueue.AddLast(cmd);
    }

    /// <summary>
    /// Clear less prioritized Commands queue and cancel current executing Command.
    /// </summary>
    /// <param name="isQueueOnly">Doesn't cancel current Command if true</param>
    /// <param name="isValidInput">Cancels validate tween of current Command if false</param>
    /// <param name="threshold">Clears Commands having equal or less priority than this value</param>
    public override void ClearAll(bool isQueueOnly = false, bool isValidInput = false, int threshold = 100)
    {
        cmdQueue = new LinkedList<ICommand>(cmdQueue.Where(cmd => cmd.IsPriorTo(threshold)));

        if (currentCommand != null)
        {
            if (isQueueOnly || currentCommand.IsPriorTo(threshold) && !isValidInput)
            {
                currentCommand.CancelValidate();
            }
            else
            {
                Cancel();
            }
        }
    }
}
