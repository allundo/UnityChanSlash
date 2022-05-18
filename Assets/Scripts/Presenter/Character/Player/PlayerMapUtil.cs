using UnityEngine;

public interface IPlayerMapUtil : IMobMapUtil
{
    bool IsExitDoorLeft { get; }
    bool IsExitDoorRight { get; }
    bool IsExitDoorBack { get; }
}

[RequireComponent(typeof(PlayerStatus))]
public class PlayerMapUtil : MobMapUtil, IPlayerMapUtil
{
    public override bool IsForwardMovable => IsMovable(dir.GetForward(onTilePos), dir);
    public override bool IsBackwardMovable => IsMovable(dir.GetBackward(onTilePos), dir.Backward);
    public override bool IsLeftMovable => IsMovable(dir.GetLeft(onTilePos), dir.Left);
    public override bool IsRightMovable => IsMovable(dir.GetRight(onTilePos), dir.Right);
    public override bool IsJumpable => IsForwardLeapable && IsMovable(GetJump, dir);

    public bool IsExitDoorLeft => dir.IsLeft(map.exitDoorDir);
    public bool IsExitDoorRight => dir.IsRight(map.exitDoorDir);
    public bool IsExitDoorBack => dir.IsSame(map.exitDoorDir);

    public void SetPosition(WorldMap map, bool isDownStairs = true)
    {
        this.map = map;
        (status as PlayerStatus).SetPosition(isDownStairs ? map.StairsBottom : map.stairsTop);
        SetObjectOn();
    }
}
