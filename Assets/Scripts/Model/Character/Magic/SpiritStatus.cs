public class SpiritStatus : MagicStatus
{
    public override MagicStatus SetShooter(IStatus shooter)
    {
        shotBy = (shooter as IMagicStatus).shotBy;
        attack = shooter.attack;
        return this;
    }
}
