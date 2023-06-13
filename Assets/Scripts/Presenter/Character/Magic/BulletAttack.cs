using System;
using UniRx;

public interface IBulletAttack : IMagicAttack
{
    IObservable<float> Hit { get; }
}

public class BulletAttack : MagicAttack, IBulletAttack
{
    public IObservable<float> Hit => hitSubject;
    protected ISubject<float> hitSubject = new Subject<float>();

    protected override void AffectTarget(IMobReactor target)
    {
        var magicStatus = status as MagicStatus;
        hitSubject.OnNext(target.Damage(Shooter.New(magicStatus.Attack, magicStatus.shotBy), data));
    }
}
