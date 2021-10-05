public abstract class ShieldInput : MobInput
{
    public GuardStateTemp guardState { get; protected set; }

    protected override void Awake()
    {
        target = GetComponent<CommandTarget>();
        map = GetComponent<MapUtil>();
        commander = new ShieldCommander(target);
    }

    protected override void SetCommands()
    {
        guardState = new GuardStateTemp(this);

        die = new DieCommand(target, 0.1f);
    }
}
