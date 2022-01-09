using UnityEngine;

[RequireComponent(typeof(EnemyStatus))]
public class EnemyMapUtil : MapUtil
{
    /// <summary>
    /// Tile position of enemy body Collider for fighting
    /// </summary>
    protected Pos onTileEnemyPos;

    public override void SetPosition(Vector3 pos, IDirection dir = null)
    {
        base.SetPosition(pos, dir);

        // Initialize enemy body collider position by the Tile position currently on
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
        map.GetTile(destPos).OnEnemy = status as EnemyStatus;
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
        map.GetTile(pos).OnEnemy = null;
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
}