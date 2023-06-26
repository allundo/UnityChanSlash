using System;
using UniRx;
using DG.Tweening;

public class LightLaserFire : MagicCommand
{
    public LightLaserFire(CommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        (attack as LightLaserAttack).SetColliderLength();
        SetOnCompleted((react as LightLaserReactor).FireSubLaser);
        return true;
    }
}

public class SubLaserFire : MagicCommand
{
    public SubLaserFire(CommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        (attack as LightLaserAttack).SetColliderLength();
        return true;
    }
}

public class LightLaserActive : MagicCommand
{
    public LightLaserActive(CommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        completeTween = attack.AttackSequence(duration).Play();
        (react as IMortalReactor).ReduceHP();
        return true;
    }
}

public class LightLaserDie : MagicCommand
{
    public LightLaserDie(CommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        react.OnDie();
        return ObservableComplete(); // Don't validate input.
    }
}
