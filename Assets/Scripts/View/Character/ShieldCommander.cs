using UnityEngine;
using UniRx;
using System;

public class ShieldCommander : Commander
{

    /// <summary>
    /// Executing Command. null if no Command is executing.
    /// </summary>
    public override ICommand currentCommand
    {
        get { return CurrentCommand.Value; }
        protected set { CurrentCommand.Value = value; }
    }

    private IReactiveProperty<ICommand> CurrentCommand = new ReactiveProperty<ICommand>(null);
    public IObservable<ICommand> CurrentObservable => CurrentCommand;

    public ShieldCommander(GameObject target) : base(target) { }
}
