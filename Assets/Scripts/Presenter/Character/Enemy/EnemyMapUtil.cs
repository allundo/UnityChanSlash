using UnityEngine;

[RequireComponent(typeof(EnemyStatus))]
public class EnemyMapUtil : MobMapUtil
{
    /// <summary>
    /// Tile position of enemy body Collider for fighting
    /// </summary>
    protected Pos onTileEnemyPos;

    public override void OnActive()
    {
        SetObjectOn();
        SetOnEnemy(onTilePos);
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
    public Pos SetOnEnemy() => SetOnEnemy(tf.position);

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
        map.GetTile(destPos).OnEnemy = status as IEnemyStatus;
        onTileEnemyPos = destPos;
        return destPos;
    }

    /// <summary>
    /// Remove enemy status info set on the Tile previously on
    /// </summary>
    public void RemoveOnEnemy() { RemoveOnEnemy(onTileEnemyPos); }

    /// <summary>
    /// Remove enemy status info set on the Tile specified by Vector3 position
    /// </summary>
    /// <param name="pos">Vector3 Tile position</param>
    public void ResetOnEnemy(Vector3 pos) { RemoveOnEnemy(map.MapPos(pos)); }

    /// <summary>
    /// Remove enemy status info set on the Tile specified by Vector3 position
    /// </summary>
    /// <param name="pos">Pos unit Tile position</param>
    public void RemoveOnEnemy(Pos pos)
    {
        ITile tile = map.GetTile(pos);
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
    /// <param name="pos">Pos unit Tile position</param>
    public override void RemoveObjectOn(Pos pos)
    {
        if (mobStatus.isOnGround)
        {
            map.GetTile(pos).OnCharacterDest = null;
        }
        else
        {
            map.GetTile(pos).AboveEnemy = null;
        }
    }
}