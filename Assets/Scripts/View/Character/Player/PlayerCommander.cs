using UniRx;
using System;

public class PlayerCommander : ShieldCommander
{
    protected PlayerAnimator anim;

    public PlayerCommander(PlayerCommandTarget target) : base(target)
    {
        anim = target.anim as PlayerAnimator;
    }

    public override void EnqueueCommand(Command cmd, bool dispatch = false)
    {
        base.EnqueueCommand(cmd, dispatch);
        if (anim.cancel.Bool) CheckCancel();
    }

    protected override void Subscribe(IObservable<Unit> execObservable)
    {
        if (execObservable == null) return;

        execDisposable = execObservable.Subscribe(_ => CheckCancel(), () => DispatchCommand()).AddTo(targetObject);
    }

    protected void CheckCancel()
    {
        if (cmdQueue.Count > 0 && cmdQueue.Peek() is PlayerAttack)
        {
            Cancel();
        }
    }
}