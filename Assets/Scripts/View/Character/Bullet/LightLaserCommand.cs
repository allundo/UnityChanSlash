using System;
using UniRx;
using DG.Tweening;

public class LightLaserFire : BulletCommand
{
    public LightLaserFire(ICommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        (attack as LightLaserAttack).SetColliderLength();
        SetOnCompleted((react as LightLaserReactor).FireSubLaser);
        return true;
    }
}

public class SubLaserFire : BulletCommand
{
    public SubLaserFire(ICommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        (attack as SubLaserAttack).SetColliderLength();
        return true;
    }
}

public class LightLaserActive : BulletCommand
{
    public LightLaserActive(ICommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        completeTween = attack.AttackSequence(duration).Play();
        (react as IBulletReactor).ReduceHP();
        return true;
    }
}

public class LightLaserDie : BulletCommand
{
    public LightLaserDie(ICommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        react.OnDie();
        return ObservableComplete(); // Don't validate input.
    }
}
