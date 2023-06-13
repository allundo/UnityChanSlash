
public class DarkHoundAttack : BulletAttack
{
    protected override void AffectTarget(IMobReactor target)
    {
        var magicStatus = status as MagicStatus;
        hitSubject.OnNext(target.Damage(Shooter.New(magicStatus.Attack, magicStatus.shotBy, Direction.Convert(transform.forward)), data));
    }
}
