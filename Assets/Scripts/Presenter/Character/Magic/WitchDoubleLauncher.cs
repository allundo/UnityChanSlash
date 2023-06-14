public class WitchDoubleLauncher : Launcher
{
    protected bool isBackStep = true;
    public WitchDoubleLauncher(IStatus status, bool isBackStep = true) : base(status, MagicType.WitchDouble)
    {
        this.isBackStep = isBackStep;
    }

    public override void Fire()
    {
        (bulletGenerator.Fire(status) as WitchDoubleStatus).SetBackStep(isBackStep);
    }
}