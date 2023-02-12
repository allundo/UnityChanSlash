using System;
using UniRx;
using System.Collections.Generic;
using DG.Tweening;

public class MobCommand : Command
{
    protected IMobReactor mobReact;
    protected IMobMapUtil mobMap;
    public MobCommand(ICommandTarget target, float duration, float validateTiming = 0.5f) : base(target, duration, validateTiming)
    {
        mobReact = react as IMobReactor;
        mobMap = map as IMobMapUtil;
    }

    public MobCommand(IInput input, Tween playing, Tween complete, List<Action> onCompleted = null) : base(input, playing, complete, onCompleted) { }
}

public class DieCommand : MobCommand
{
    public DieCommand(ICommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        mobMap.RemoveObjectOn();
        mobReact.OnDie();

        return ExecOnCompleted(() => mobReact.OnDisappear()); // Don't validate input.
    }
}

public interface IIcedCommand : ICommand { }

public class IcedCommand : MobCommand, IIcedCommand
{
    public override int priority => 20;
    private float framesToMelt;

    public IcedCommand(ICommandTarget target, float duration) : base(target, duration, 0.98f)
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
