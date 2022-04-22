public class WitchDoubleLauncher : Launcher
{
    protected bool isBackStep = true;
    public WitchDoubleLauncher(IStatus status, bool isBackStep = true) : base(status, BulletType.WitchDouble)
    {
        this.isBackStep = isBackStep;
    }

    public override void Fire()
    {
        (bulletGenerator.Fire(status.Position, status.dir, status.attack) as WitchDoubleStatus).SetBackStep(isBackStep);
    }
}
