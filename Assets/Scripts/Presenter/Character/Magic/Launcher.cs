using DG.Tweening;

public interface ILauncher : IAttack
{
    void Fire();
}

public class Launcher : ILauncher
{
    protected MagicGenerator bulletGenerator;
    protected IStatus status;

    public Launcher(IStatus status, MagicType type)
    {
        this.status = status;
        bulletGenerator = SpawnHandler.Instance.GetMagicGenerator(type);
    }

    public virtual Sequence AttackSequence(float attackDuration)
        => DOTween.Sequence()
            .AppendInterval(attackDuration * 0.3f)
            .AppendCallback(Fire)
            .SetUpdate(false);

    public virtual void Fire() => bulletGenerator.Fire(status);
}
