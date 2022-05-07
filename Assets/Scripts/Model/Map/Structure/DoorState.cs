
public class DoorState : HandleState
{
    public DoorState(ItemType type = ItemType.Null) : base(type)
    { }

    public override bool IsLocked => lockType.Value != ItemType.Null;

    public bool Unlock(ItemType key)
    {
        if (key != lockType.Value) return false;

        lockType.Value = ItemType.Null;
        return true;
    }

    public override bool IsControllable => State.Value == StateEnum.OPEN && !IsCharacterOn || State.Value == StateEnum.CLOSE;

    public bool IsCharacterOn => onCharacterDest != null;
    public IStatus onCharacterDest = null;
}

