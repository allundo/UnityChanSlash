public class MagicAndDouble : Magic
{
    public WitchDoubleLauncher backStepWitchLauncher { get; protected set; }
    public WitchDoubleLauncher jumpOverWitchLauncher { get; protected set; }

    protected override void Start()
    {
        base.Start();

        WitchStatus status = GetComponent<WitchStatus>();

        backStepWitchLauncher = new WitchDoubleLauncher(status, true);
        jumpOverWitchLauncher = new WitchDoubleLauncher(status, false);
    }
}
