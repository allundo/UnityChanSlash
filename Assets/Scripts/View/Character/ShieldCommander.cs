using UniRx;
using System;

public class ShieldCommander : Commander
{
    public ShieldCommander(CommandTarget target) : base(target) { }

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
}