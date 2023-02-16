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
        bulletGenerator = SpawnHandler.Instance.GetBulletGenerator(type);
    }

    public virtual Sequence AttackSequence(float attackDuration)
        => DOTween.Sequence()
            .AppendInterval(attackDuration * 0.3f)
            .AppendCallback(Fire)
            .AppendInterval(attackDuration * 0.7f)
            .SetUpdate(false);

    public virtual void Fire() => bulletGenerator.Fire(status);
}
