using System;
public class UndeadInput
{
    private Action<ICommand> interrupt;

    private ICommand sleep;
    private ICommand quickSleep;

    public UndeadInput(ICommandTarget target, Action<ICommand> interrupt)
        : this(interrupt, new UndeadSleep(target), new UndeadQuickSleep(target))
    { }

    public UndeadInput(Action<ICommand> interrupt, ICommand sleep, ICommand quickSleep)
    {
        this.interrupt = interrupt;
        this.sleep = sleep;
        this.quickSleep = quickSleep;
    }

    public void InterruptSleep() => interrupt(sleep);
    public void OnActive(EnemyStatus.ActivateOption option)
    {
        if (option.isSleeping) interrupt(quickSleep);
    }

}