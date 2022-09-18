using DG.Tweening;
using System;
using UniRx;


public class MobCommand : Command
{
    protected IMobReactor mobReact;
    protected IMobMapUtil mobMap;
    public MobCommand(ICommandTarget target, float duration, float validateTiming = 0.5f) : base(target, duration, validateTiming)
    {
        mobReact = react as IMobReactor;
        mobMap = map as IMobMapUtil;
    }
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
    private float icingFrames;

    public IcedCommand(ICommandTarget target, float duration) : base(target, duration, 0.98f)
    {
        icingFrames = duration;
    }

    protected override bool Action()
    {
        anim.Pause();
        mobReact.Iced(icingFrames);
        completeTween = tweenMove.DelayedCall(1f, anim.Resume).Play();
        SetOnCompleted(() => mobReact.Melt());
        return true;
    }
}
