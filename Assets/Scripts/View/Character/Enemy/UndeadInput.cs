using System;
public class UndeadInput
{
    private Action<ICommand> interrupt;

    private ICommand sleep;
    private ICommand quickSleep;

    public UndeadInput(ICommandTarget target, Action<ICommand> interrupt)
    {
        this.interrupt = interrupt;
        sleep = new UndeadSleep(target, 300f, new Resurrection(target, 64f));
        quickSleep = new UndeadQuickSleep(target);
    }

    public void InterruptSleep() => interrupt(sleep);
    public void OnActive(EnemyStatus.ActivateOption option)
    {
        if (option.isSleeping) interrupt(quickSleep);
    }

}