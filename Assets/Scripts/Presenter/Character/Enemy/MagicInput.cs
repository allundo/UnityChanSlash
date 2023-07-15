using UnityEngine;
using UniRx;

public class MagicInput : InputHandler
{
    [SerializeField] protected float fireFrames = 28f;
    [SerializeField] protected float moveFrames = 28f;

    protected ICommand fire;
    protected ICommand moveForward;

    protected override void SetCommands()
    {
        die = new MagicDie(target, 28f);
        fire = new MagicFire(target, fireFrames, die);
        moveForward = new MagicMove(target, moveFrames, die);
    }

    protected override void Start()
    {
        target.interrupt.Subscribe(data =>
        {
            Interrupt(data.cmd, data.isCancel, data.isQueueClear);
            if (data.cmd == die) DisableInput();
        })
        .AddTo(this);

        target.validate.Subscribe(triggerOnly => ValidateInput(triggerOnly)).AddTo(this);
    }

    public override void OnActive()
    {
        ValidateInput();
        Interrupt(fire);
    }
    public override ICommand InterruptDie()
    {
        Interrupt(die);
        DisableInput();
        return die;
    }
    protected override ICommand GetCommand() => moveForward;
}
