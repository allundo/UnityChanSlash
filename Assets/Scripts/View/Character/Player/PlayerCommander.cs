using UnityEngine;

public class PlayerCommander : ShieldCommander
{
    public PlayerCommander(CommandTarget target) : base(target) { }

    public bool IsAttack => currentCommand is PlayerAttack;
}
