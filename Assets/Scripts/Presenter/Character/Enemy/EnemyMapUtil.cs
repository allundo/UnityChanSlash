using UnityEngine;

public interface IEnemyMapUtil : IMobMapUtil
{
    void OnActive(bool isSleeping);
}

[RequireComponent(typeof(EnemyStatus))]
public class EnemyMapUtil : MobMapUtil, IEnemyMapUtil
{
    /// <summary>
    /// Tile position of enemy body Collider for fighting
    /// </summary>
    protected Pos onTileEnemyPos;

    public override void OnActive()
    {
        base.OnActive();
        SetOnEnemy(onTilePos);
    }

    public void OnActive(bool isSleeping)
    {
        if (isSleeping)
        {
            map = GameManager.Instance.worldMap;
            onTilePos = map.MapPos(transform.position);
            return;
        }

        OnActive();
    }

    public override void ResetTile()
    {
        RemoveObjectOn();
        RemoveOnEnemy();
    }

    /// <summary>
    /// Set enemy status info to the Tile currently on
    /// </summary>
    /// <returns>Current map Pos</returns>
    public Pos SetOnEnemy() => SetOnEnemy(transform.position);

    /// <summary>
    /// Set enemy status info to the Tile specified by Vector3 position
    /// </summary>
    /// <param name="destPos">Vector3 position of destination Tile</param>
    /// <returns>destPos</returns>
    public Pos SetOnEnemy(Vector3 destPos) => SetOnEnemy(map.MapPos(destPos));

    /// <summary>
    /// Set enemy status info to the Tile specified by Pos unit
    /// </summary>
    /// <param name="destPos">Tile map position of destination</param>
    /// <returns>destPos</returns>
    public Pos SetOnEnemy(Pos destPos)
    {
        ITile tile = map.GetTile(destPos);

        if (mobStatus.isOnGround || tile.OnEnemy == null)
        {
            tile.OnEnemy = status as IEnemyStatus;
        }

        onTileEnemyPos = destPos;
        return destPos;
    }

    /// <summary>
    /// Remove enemy status info set on the Tile specified by Vector3 position
    /// </summary>
    public void RemoveOnEnemy()
    {
        ITile tile = map.GetTile(onTileEnemyPos);
        if (tile.OnEnemy == status) tile.OnEnemy = null;
    }

    /// <summary>
    /// Apply RemoveOnEnemy at previous Tile and SetOnEnemy to the Tile currently on.
    /// </summary>
    public Pos MoveOnEnemy()
    {
        RemoveOnEnemy();
        return SetOnEnemy();
    }

    /// <summary>
    /// Apply RemoveOnEnemy at previous Tile and SetOnEnemy to the Tile of dest Pos.
    /// </summary>
    public Pos MoveOnEnemy(Pos destPos)
    {
        RemoveOnEnemy();
        return SetOnEnemy(destPos);
    }

    /// <summary>
    /// Set current on tile and IsObjectOn flag to the Tile specified by Pos unit
    /// </summary>
    /// <param name="destPos">Tile map position of destination</param>
    /// <returns>destPos</returns>
    public override Pos SetObjectOn(Pos destPos)
    {
        if (mobStatus.isOnGround)
        {
            map.GetTile(destPos).OnCharacterDest = status;
        }
        else
        {
            map.GetTile(destPos).AboveEnemy = status as IEnemyStatus;
        }

        onTilePos = destPos;
        return destPos;
    }

    /// <summary>
    /// Set IsObjectOn flag FALSE to the Tile specified by Vector3 position
    /// </summary>
    public override void RemoveObjectOn()
    {
        ITile tile = map.GetTile(onTilePos);
        if (mobStatus.isOnGround)
        {
            if (tile.OnCharacterDest == status) tile.OnCharacterDest = null;
        }
        else
        {
            if (tile.AboveEnemy == status) tile.AboveEnemy = null;
        }
    }
}