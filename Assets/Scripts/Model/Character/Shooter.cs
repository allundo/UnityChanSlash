public class Shooter : Attacker
{
    public static IAttacker New(float attack, IStatus status)
    {
        if (status is IEnemyStatus) return new EnemyShooter(attack, status as IEnemyStatus);
        if (status is PlayerStatus) return new PlayerShooter(attack, status as PlayerStatus);
        return new Shooter(attack, status);
    }

    protected Shooter(float attack, IStatus status) : base(attack, status.dir, status.Name) { }
}
