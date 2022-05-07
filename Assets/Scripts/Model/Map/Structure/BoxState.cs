public class BoxState : HandleState
{
    public override bool IsLocked => false; // TODO: Key lock isn't implemented for now.
    public override bool IsControllable => State.Value == StateEnum.OPEN || State.Value == StateEnum.CLOSE;
}
