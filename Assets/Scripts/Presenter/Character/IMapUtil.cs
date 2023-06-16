using UnityEngine;

public interface IMapUtil
{
    IDirection dir { get; }
    Pos onTilePos { get; }
    Transform transform { get; }

    void OnActive();

    Vector3 WorldPos(Pos pos);

    void TurnLeft();
    void TurnRight();
    void TurnBack();

    /// <summary>
    /// Vector3 to Forward from current direction
    /// </summary>
    /// <param name="distance">magnitude by TILE_UNIT</param>
    Vector3 GetForwardVector(float distance = 1f);

    /// <summary>
    /// Vector3 to Backward from current direction
    /// </summary>
    /// <param name="distance">magnitude by TILE_UNIT</param>

    Vector3 GetBackwardVector(float distance = 1f);
    /// <summary>
    /// Vector3 to Right from current direction
    /// </summary>
    /// <param name="distance">magnitude by TILE_UNIT</param>

    Vector3 GetRightVector(float distance = 1f);
    /// <summary>
    /// Vector3 to Left from current direction
    /// </summary>
    /// <param name="distance">magnitude by TILE_UNIT</param>
    Vector3 GetLeftVector(float distance = 1f);

    ITile GetTile(Pos destPos);
    ITile OnTile { get; }
    ITile ForwardTile { get; }
    ITile BackwardTile { get; }
    ITile RightTile { get; }
    ITile LeftTile { get; }
    ITile JumpTile { get; }

    bool IsObjectOn(Pos destPos);
    bool IsViewable(Pos destPos);

    Vector3 CurrentVec3Pos { get; }
    Vector3 DestVec { get; }
    Vector3 DestVec3Pos { get; }

    Pos GetForward { get; }
    Pos GetLeft { get; }
    Pos GetRight { get; }
    Pos GetBackward { get; }
    Pos GetJump { get; }

    bool IsOnPlayer(Pos pos);
    bool IsPlayerFound();
    bool IsPlayerFound(Pos pos);

    bool IsPlayerForward { get; }
    bool IsPlayerLeft { get; }
    bool IsPlayerRight { get; }
    bool IsPlayerBackward { get; }

    /// <summary>
    /// Set current on tile and IsObjectOn flag to the Tile currently on
    /// </summary>
    /// <returns>Current map Pos</returns>
    Pos SetObjectOn();

    /// <summary>
    /// Set current on tile and IsObjectOn flag to the Tile specified by Vector3 position
    /// </summary>
    /// <param name="destPos">Vector3 position of destination Tile</param>
    /// <returns>destPos</returns>
    Pos SetObjectOn(Vector3 destPos);

    /// <summary>
    /// Set current on tile and IsObjectOn flag to the Tile specified by Pos unit
    /// </summary>
    /// <param name="destPos">Tile map position of destination</param>
    /// <returns>destPos</returns>
    Pos SetObjectOn(Pos destPos);

    /// <summary>
    /// Apply RemoveObjectOn at current Tile and SetObjectOn to the dest Pos.
    /// </summary>
    /// <param name="destPos">Vector3 position of destination Tile</param>
    /// <returns>destPos</returns>
    Pos MoveObjectOn(Vector3 destPos);

    /// <summary>
    /// Apply RemoveObjectOn at current Tile and SetObjectOn to the dest Pos.
    /// </summary>
    /// <param name="destPos">Tile map position of destination</param>
    /// <returns>destPos</returns>
    Pos MoveObjectOn(Pos destPos);
}
