using UnityEngine;
using System.Collections.Generic;

public interface IMobMapUtil : IMapUtil
{
    void ResetTile();

    /// <summary>
    /// Set IsObjectOn flag FALSE to the Tile currently on
    /// </summary>
    void RemoveObjectOn();

    /// <summary>
    /// Set IsObjectOn flag FALSE to the Tile specified by Vector3 position
    /// </summary>
    /// <param name="pos">Vector3 Tile position</param>
    void RemoveObjectOn(Vector3 pos);

    /// <summary>
    /// Set IsObjectOn flag FALSE to the Tile specified by Vector3 position
    /// </summary>
    /// <param name="pos">Pos unit Tile position</param>
    void RemoveObjectOn(Pos pos);

    bool IsMovable(Pos destPos, IDirection dir = null);
    bool IsLeapable(Pos destPos);

    bool IsForwardMovable { get; }
    bool IsForwardLeapable { get; }
    bool IsBackwardMovable { get; }
    bool IsLeftMovable { get; }
    bool IsRightMovable { get; }
    bool IsJumpable { get; }

    Pos SearchSpaceNearBy(int range = 2, List<Pos> exceptFor = null);
    Pos SearchSpaceNearBy(Pos targetPos, int range = 2, List<Pos> exceptFor = null);
}

/// <summary>
/// Map の Tile の状態を更新するメソッドを公開 <br>
/// Map の状態とキャラクターの位置・向きに応じた情報を提供するメソッドを公開
/// </summary>
[RequireComponent(typeof(MobStatus))]
public class MobMapUtil : MapUtil, IMobMapUtil
{
    protected IMobStatus mobStatus;

    protected override void Awake()
    {
        base.Awake();
        mobStatus = status as IMobStatus;
    }

    public virtual void ResetTile() => RemoveObjectOn();
    public bool IsMovable(Pos destPos, IDirection dir = null) => map.GetTile(destPos).IsEnterable(dir);
    public bool IsLeapable(Pos destPos) => map.GetTile(destPos).IsLeapable;

    public virtual bool IsForwardMovable => IsMovable(dir.GetForward(onTilePos));
    public bool IsForwardLeapable => IsLeapable(dir.GetForward(onTilePos));
    public virtual bool IsBackwardMovable => IsMovable(dir.GetBackward(onTilePos));
    public virtual bool IsLeftMovable => IsMovable(dir.GetLeft(onTilePos));
    public virtual bool IsRightMovable => IsMovable(dir.GetRight(onTilePos));
    public virtual bool IsJumpable => IsForwardLeapable && IsMovable(GetJump);
    public Pos SearchSpaceNearBy(int range = 2, List<Pos> exceptFor = null) => SearchSpaceNearBy(GameManager.Instance.PlayerPos, range, exceptFor);

    public Pos SearchSpaceNearBy(Pos targetPos, int range = 2, List<Pos> exceptFor = null)
    {
        var destCandidates = new List<Pos>();

        for (int j = targetPos.y - range; j < targetPos.y + range; j++)
        {
            for (int i = targetPos.x - range; i < targetPos.x + range; i++)
            {
                var pos = new Pos(i, j);
                if (map.GetTile(pos).IsEnterable()) destCandidates.Add(pos);
            }
        }

        exceptFor?.ForEach(pos => destCandidates.Remove(pos));

        return destCandidates.Count > 0 ? destCandidates.GetRandom() : new Pos();
    }

    /// <summary>
    /// Set current on tile and IsObjectOn flag to the Tile specified by Pos unit
    /// </summary>
    /// <param name="destPos">Tile map position of destination</param>
    /// <returns>destPos</returns>
    public override Pos SetObjectOn(Pos destPos)
    {
        if (mobStatus.isOnGround) map.GetTile(destPos).OnCharacterDest = status;
        return base.SetObjectOn(destPos);
    }

    /// <summary>
    /// Set IsObjectOn flag FALSE to the Tile currently on
    /// </summary>
    public void RemoveObjectOn() => RemoveObjectOn(onTilePos);

    /// <summary>
    /// Set IsObjectOn flag FALSE to the Tile specified by Vector3 position
    /// </summary>
    /// <param name="pos">Vector3 Tile position</param>
    public void RemoveObjectOn(Vector3 pos) => RemoveObjectOn(map.MapPos(pos));

    /// <summary>
    /// Set IsObjectOn flag FALSE to the Tile specified by Vector3 position
    /// </summary>
    /// <param name="pos">Pos unit Tile position</param>
    public virtual void RemoveObjectOn(Pos pos)
    {
        if (mobStatus.isOnGround) map.GetTile(pos).OnCharacterDest = null;
    }

    /// <summary>
    /// Apply RemoveObjectOn at current Tile and SetObjectOn to the dest Pos.
    /// </summary>
    public override Pos MoveObjectOn(Pos destPos)
    {
        RemoveObjectOn();
        return SetObjectOn(destPos);
    }
}
