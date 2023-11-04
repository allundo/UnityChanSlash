
public class DoorState : HandleState
{
    public DoorState(ItemType type = ItemType.Null) : base(type)
    { }

    protected override ActiveMessageData lockedMessage
        => new ActiveMessageData("扉には鍵がかかっている");

    public override bool IsLocked => lockType.Value != ItemType.Null;
    public bool isBroken { get; protected set; } = false;

    public bool Unlock(ItemType key)
    {
        if (key != lockType.Value) return false;

        lockType.Value = ItemType.Null;
        ActiveMessageController.Instance.KeyLockOpen();
        return true;
    }

    public override bool IsControllable => !isBroken && State.Value == StateEnum.OPEN && !IsCharacterOn || State.Value == StateEnum.CLOSE;

    public bool IsCharacterOn => onCharacterDest != null;
    public IStatus onCharacterDest = null;

    public void Break()
    {
        isBroken = true;
        lockType.Value = ItemType.Null;
        state.Value = StateEnum.DESTRUCT;
    }
}
