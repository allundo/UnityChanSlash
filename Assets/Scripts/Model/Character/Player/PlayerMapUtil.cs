public class PlayerMapUtil : MapUtil
{
    public override bool IsForwardMovable => IsMovable(dir.GetForward(onTilePos), dir);
    public override bool IsBackwardMovable => IsMovable(dir.GetBackward(onTilePos), dir.Backward);
    public override bool IsLeftMovable => IsMovable(dir.GetLeft(onTilePos), dir.Left);
    public override bool IsRightMovable => IsMovable(dir.GetRight(onTilePos), dir.Right);
    public override bool IsJumpable => IsForwardLeapable && IsMovable(dir.GetForward(dir.GetForward(onTilePos)), dir);
}
