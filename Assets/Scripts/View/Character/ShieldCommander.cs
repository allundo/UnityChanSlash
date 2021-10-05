
public class ShieldCommander : MobCommander
{
    public ShieldCommander(CommandTarget target) : base(target) { }

    public bool IsGuard => currentCommand is ShieldCommand;
    public bool IsFightValid => IsIdling || IsGuard;
}
