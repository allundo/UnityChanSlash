using DG.Tweening;

public interface ILauncher : IAttack
{
    void Fire();
}

public class Launcher : ILauncher
{
    protected BulletGenerator bulletGenerator;
    protected IStatus status;

    public Launcher(IStatus status, BulletType type)
    {
        this.status = status;
        bulletGenerator = GameManager.Instance.GetBulletGenerator(type);
    }

    public virtual Tween AttackSequence(float attackDuration)
        => DOVirtual.DelayedCall(attackDuration * 0.3f, Fire, false);

    public virtual void Fire() => bulletGenerator.Fire(status.Position, status.dir, status.attack);
}
