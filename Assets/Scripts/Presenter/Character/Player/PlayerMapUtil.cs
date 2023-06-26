using UnityEngine;
using System.Collections.Generic;
using UniRx;
using System;

public interface IPlayerMapUtil : IMobMapUtil
{
    bool IsInPit { get; }
    bool IsPitJumpable { get; }
    IObservable<IDirection> Dir { get; }
    void SetStartPos(WorldMap map, KeyValuePair<Pos, IDirection> initPos);
}

[RequireComponent(typeof(PlayerStatus))]
public class PlayerMapUtil : MobMapUtil, IPlayerMapUtil
{
    protected PlayerStatus playerStatus;

    protected override void Awake()
    {
        base.Awake();
        playerStatus = status as PlayerStatus;
    }
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
    public bool IsInPit => playerStatus.isInPit;
    public override bool IsForwardMovable => IsMovable(dir.GetForward(onTilePos), dir) && !IsInPit;
    public override bool IsBackwardMovable => IsMovable(dir.GetBackward(onTilePos), dir.Backward) && !IsInPit;
    public override bool IsLeftMovable => IsMovable(dir.GetLeft(onTilePos), dir.Left) && !IsInPit;
    public override bool IsRightMovable => IsMovable(dir.GetRight(onTilePos), dir.Right) && !IsInPit;
    public override bool IsJumpable => IsForwardLeapable && IsMovable(GetJump, dir);
    public bool IsPitJumpable => IsMovable(dir.GetForward(onTilePos), dir);

    public void SetFloorStartPos(WorldMap map, bool isDownStairs = true)
        => SetStartPos(map, isDownStairs ? map.stairsBottom : map.stairsTop);

    public void SetStartPos(WorldMap map, KeyValuePair<Pos, IDirection> initPos)
    {
        this.map = map;
        playerStatus.SetPosition(map, initPos);
        SetObjectOn();
    }

    public override Pos SetObjectOn(Pos destPos)
    {
        playerStatus.isInPit = map.GetTile(destPos) is Pit;
        return base.SetObjectOn(destPos);
    }

    public override Pos MoveObjectOn(Pos destPos)
    {
        playerStatus.counter.IncStep();
        return base.MoveObjectOn(destPos);
    }
}
