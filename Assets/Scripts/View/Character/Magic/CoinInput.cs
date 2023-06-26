using UniRx;

public class CoinInput : MagicInput
{
    protected CoinDrop drop;
    protected override void SetCommands()
    {
        drop = new CoinDrop(target, 56f);
        fire = new CoinFire(target, 28f, drop);
        moveForward = new CoinMove(target, 28f, drop);
        die = new MagicDie(target, 28f);
    }

    protected override void Start()
    {
        target.interrupt.Subscribe(data =>
        {
            Interrupt(data.cmd, data.isCancel, data.isQueueClear);
            if (data.cmd == drop) DisableInput();
        })
        .AddTo(this);

        target.validate.Subscribe(triggerOnly => ValidateInput(triggerOnly)).AddTo(this);
    }
}
