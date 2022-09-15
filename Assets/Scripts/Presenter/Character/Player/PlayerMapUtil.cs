using UnityEngine;

public interface IPlayerMapUtil : IMobMapUtil
{
    bool isInPit { get; }
    bool IsPitJumpable { get; }
}

[RequireComponent(typeof(PlayerStatus))]
public class PlayerMapUtil : MobMapUtil, IPlayerMapUtil
{
    public bool isInPit { get; protected set; } = false;
    public override bool IsForwardMovable => IsMovable(dir.GetForward(onTilePos), dir) && !isInPit;
    public override bool IsBackwardMovable => IsMovable(dir.GetBackward(onTilePos), dir.Backward) && !isInPit;
    public override bool IsLeftMovable => IsMovable(dir.GetLeft(onTilePos), dir.Left) && !isInPit;
    public override bool IsRightMovable => IsMovable(dir.GetRight(onTilePos), dir.Right) && !isInPit;
    public override bool IsJumpable => IsForwardLeapable && IsMovable(GetJump, dir);
    public bool IsPitJumpable => IsMovable(dir.GetForward(onTilePos), dir);

    public void SetPosition(WorldMap map, bool isDownStairs = true)
    {
        this.map = map;
        (status as PlayerStatus).SetPosition(isDownStairs ? map.StairsBottom : map.stairsTop);
        SetObjectOn();
    }

    public override Pos SetObjectOn(Pos destPos)
    {
        isInPit = map.GetTile(destPos) is Pit;
        return base.SetObjectOn(destPos);
    }
}
