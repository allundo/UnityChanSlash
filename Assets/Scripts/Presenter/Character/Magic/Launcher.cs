using DG.Tweening;

public interface ILauncher
{
    Sequence FireSequence(float fireDuration);
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

    public virtual Sequence FireSequence(float fireDuration)
        => DOTween.Sequence()
            .AppendInterval(fireDuration * 0.3f)
            .AppendCallback(Fire)
            .SetUpdate(false);

    public virtual void Fire() => bulletGenerator.Fire(status);
}
