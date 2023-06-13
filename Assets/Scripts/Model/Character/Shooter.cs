public class Shooter : Attacker
{
    public static IAttacker New(float attack, IStatus status, IDirection dir = null)
    {
        if (status is IEnemyStatus) return new EnemyShooter(attack, status as IEnemyStatus);
        if (status is PlayerStatus) return new PlayerShooter(attack, status as PlayerStatus);
        return new Shooter(attack, status, dir);
    }

    protected Shooter(float attack, IStatus status, IDirection dir = null) : base(attack, dir ?? status.dir, status.Name) { }
}
