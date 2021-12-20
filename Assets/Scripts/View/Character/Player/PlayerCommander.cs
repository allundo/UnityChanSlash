using UniRx;
using System;

public class PlayerCommander : Commander
{
    protected PlayerAnimator anim;

    /// <summary>
    /// Executing Command. null if no Command is executing.
    /// </summary>
    public override Command currentCommand
    {
        get { return CurrentCommand.Value; }
        protected set { CurrentCommand.Value = value; }
    }

    private IReactiveProperty<Command> CurrentCommand = new ReactiveProperty<Command>(null);
    public IObservable<Command> CurrentObservable => CurrentCommand;

    public PlayerCommander(PlayerCommandTarget target) : base(target)
    {
        anim = target.anim as PlayerAnimator;
    }

    public override void EnqueueCommand(Command cmd)
    {
        base.EnqueueCommand(cmd);
        if (anim.cancel.Bool) CheckCancel(); // Cancel current cancelable Attack if newly enqueued command is Attack
    }

    protected override void Subscribe(IObservable<Unit> execObservable)
    {
        if (execObservable == null) return;

        execDisposable = execObservable
            .Subscribe(
                _ => CheckCancel(), // Cancel current Attack if next is also Attack on cancelation timing
                () => DispatchCommand()
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
}