using DG.Tweening;

public class Launcher : IAttack
{
    protected BulletGenerator bulletGenerator;
    protected IStatus status;

    public Launcher(IStatus status, BulletType type)
    {
        this.status = status;
        bulletGenerator = GameManager.Instance.GetBulletGenerator(type);
    }

    public virtual Tween AttackSequence(float attackDuration)
    {
        return
            DOVirtual.DelayedCall(
                attackDuration * 0.3f,
                () => bulletGenerator.Fire(status.Position, status.dir, status.Attack),
                false
            );
    }
}
