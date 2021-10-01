using UnityEngine;

[RequireComponent(typeof(ShieldAnimator))]
public abstract class ShieldCommander : MobCommander
{
    public bool IsGuard => currentCommand is ShieldCommand;
    public bool IsFightValid => IsIdling || IsGuard;

}
