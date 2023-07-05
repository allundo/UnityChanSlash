using UniRx;
using System;
using System.Linq;
using System.Collections.Generic;

public class PlayerCommander : ShieldCommander
{
    protected PlayerAnimator anim;

    private ICommand guard;

    protected ISubject<ICommand> commandComplete = new Subject<ICommand>();
    public IObservable<ICommand> CommandComplete => commandComplete;

    private bool isCancelable = false;
    public void SetCancel() => isCancelable = true;

    public PlayerCommander(PlayerCommandTarget target) : base(target.gameObject)
    {
        anim = target.anim as PlayerAnimator;
        guard = new PlayerGuardCommand(target, 6000f);
    }

    public override void EnqueueCommand(ICommand cmd)
    {
        base.EnqueueCommand(cmd);

        // Cancel current cancelable Attack command if next command is also Attack during cancellable.
        if (isCancelable) CheckCancel();
    }

    public void SetGuard(bool isGuardOn)
    {
        if (isGuardOn)
        {
            EnqueueCommand(guard);
        }
        else
        {
            if (currentCommand is GuardCommand) Cancel();
            cmdQueue.Remove(guard);
        }
    }

    protected override void Subscribe(ICommand cmd)
    {
        IObservable<Unit> execObservable = cmd.Execute();

        if (execObservable == null) return;

        execDisposable = execObservable
            .Subscribe(
                // Cancelable Attack commands check cancellation on specified timing and
                // cancel itself if next is also Attack command.
                _ => CheckCancel(),
                () =>
                {
                    isCancelable = false;
                    commandComplete.OnNext(cmd);
                    DispatchCommand();
                }
            )
            .AddTo(targetObject);
    }

    public override void Cancel()
    {
        isCancelable = false;
        base.Cancel();
    }

    protected void CheckCancel()
    {
        if (cmdQueue.First?.Value is PlayerAttackCommand)
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
            if (isQueueOnly || currentCommand.IsPriorTo(threshold))
            {
                if (!isValidInput) currentCommand.CancelValidate();
            }
            else
            {
                Cancel();
            }
        }
    }
}
