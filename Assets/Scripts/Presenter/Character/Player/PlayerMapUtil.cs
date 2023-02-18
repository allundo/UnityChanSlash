using UnityEngine;
using System.Collections.Generic;
using UniRx;
using System;

public interface IPlayerMapUtil : IMobMapUtil
{
    bool isInPit { get; }
    bool IsPitJumpable { get; }
}

[RequireComponent(typeof(PlayerStatus))]
public class PlayerMapUtil : MobMapUtil, IPlayerMapUtil
{
    private IReactiveProperty<IDirection> reactiveDir = new ReactiveProperty<IDirection>(null);
    public IObservable<IDirection> Dir => reactiveDir;

    public override IDirection dir
    {
        get { return status.dir; }
        set
        {
            reactiveDir.Value = value;
            status.SetDir(value);
        }
    }
    public bool isInPit { get; protected set; } = false;
    public override bool IsForwardMovable => IsMovable(dir.GetForward(onTilePos), dir) && !isInPit;
    public override bool IsBackwardMovable => IsMovable(dir.GetBackward(onTilePos), dir.Backward) && !isInPit;
    public override bool IsLeftMovable => IsMovable(dir.GetLeft(onTilePos), dir.Left) && !isInPit;
    public override bool IsRightMovable => IsMovable(dir.GetRight(onTilePos), dir.Right) && !isInPit;
    public override bool IsJumpable => IsForwardLeapable && IsMovable(GetJump, dir);
    public bool IsPitJumpable => IsMovable(dir.GetForward(onTilePos), dir);

    public void SetFloorStartPos(WorldMap map, bool isDownStairs = true)
        => SetStartPos(map, isDownStairs ? map.StairsBottom : map.stairsTop);

    public void SetStartPos(WorldMap map, KeyValuePair<Pos, IDirection> initPos)
    {
        this.map = map;
        (status as PlayerStatus).SetPosition(map, initPos);
        SetObjectOn();
    }

    public override Pos SetObjectOn(Pos destPos)
    {
        isInPit = map.GetTile(destPos) is Pit;
        return base.SetObjectOn(destPos);
    }
}
