public class PlayerMapUtil : MapUtil
{
    public void SetPosition(bool isDownStairs)
    {
        map = GameManager.Instance.worldMap;
        SetPosition(isDownStairs ? map.stairsBottom : map.stairsTop);
    }

    public override void SetPosition() => SetPosition(map.InitPos);
    public override bool IsForwardMovable => IsMovable(dir.GetForward(onTilePos), dir);
    public override bool IsBackwardMovable => IsMovable(dir.GetBackward(onTilePos), dir.Backward);
    public override bool IsLeftMovable => IsMovable(dir.GetLeft(onTilePos), dir.Left);
    public override bool IsRightMovable => IsMovable(dir.GetRight(onTilePos), dir.Right);
    public override bool IsJumpable => IsForwardLeapable && IsMovable(GetJump, dir);
}
