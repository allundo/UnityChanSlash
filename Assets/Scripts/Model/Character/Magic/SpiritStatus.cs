public class SpiritStatus : BulletStatus
{
    public override BulletStatus SetShooter(IStatus shooter)
    {
        shotBy = (shooter as IBulletStatus).shotBy;
        attack = shooter.attack;
        return this;
    }
}
