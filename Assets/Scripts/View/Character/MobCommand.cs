using System;
using UniRx;

public class MobCommand : Command
{
    protected IMobReactor mobReact;
    protected IMobMapUtil mobMap;
    public MobCommand(CommandTarget target, float duration, float validateTiming = 0.5f) : base(target, duration, validateTiming)
    {
        mobReact = react as IMobReactor;
        mobMap = map as IMobMapUtil;
    }
}

public class DieCommand : MobCommand
{
    public DieCommand(CommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        mobMap.RemoveObjectOn();
        mobReact.OnDie();

        return ExecOnCompleted(() => mobReact.OnDisappear()); // Don't validate input.
    }
}

public interface IIcedCommand : ICommand
{
    float framesToMelt { get; }
}

public class IcedCommand : MobCommand, IIcedCommand
{
    public override int priority => 20;
    public float framesToMelt { get; protected set; }

    public IcedCommand(CommandTarget target, float duration, float validateTiming) : base(target, duration, validateTiming)
    {
        framesToMelt = duration;
    }

    protected override bool Action()
    {
        mobReact.Iced(framesToMelt);
        SetOnCompleted(() => mobReact.Melt());
        return true;
    }
}
