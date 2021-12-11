using UniRx;
using System;

public class PlayerCommander : ShieldCommander
{
    protected PlayerAnimator anim;

    public PlayerCommander(PlayerCommandTarget target) : base(target)
    {
        anim = target.anim as PlayerAnimator;
    }

    public override void EnqueueCommand(Command cmd)
    {
        base.EnqueueCommand(cmd);
        if (anim.cancel.Bool) CheckCancel();
    }

    protected override void Subscribe(IObservable<Unit> execObservable)
    {
        if (execObservable == null) return;

        execDisposable = execObservable.Subscribe(_ => CheckCancel(), () => DispatchCommand()).AddTo(targetObject);
    }

    protected void CheckCancel()
    {
        if (cmdQueue.First?.Value is PlayerAttack)
        {
            Cancel();
        }
    }
}